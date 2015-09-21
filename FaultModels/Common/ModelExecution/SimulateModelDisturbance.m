function [ObjectiveFunctionValues] = SimulateModelDisturbance(ModelFile, DesiredValue, DisturbanceAmplitude, DisturbanceSignalType, DisturbanceStartTime, DisturbanceDuration, DisturbanceUpTime, ActualValueRangeStart, ActualValueRangeEnd, ObjectiveFunction, SimulationSteps, ModelTimeStep, DesiredVariableName, ActualVariableName, DisturbanceVariableName, tStable, tLive, smoothnessStartDifference, responsivenessClose, AccelerationDisabled, ModelConfigurationFile)
    % re-configure the model
    if (ModelConfigurationFile)
        evalin('base', strcat('run(''',ModelConfigurationFile,''')'));
    end
    
    % generate the time for the desired value  
    assignin('base', DesiredVariableName, CT_GenerateConstantSignal(SimulationSteps, ModelTimeStep, DesiredValue));

    switch DisturbanceSignalType
        case 1
            assignin('base', DisturbanceVariableName, CT_GenerateTrapezoidalRampSignal(SimulationSteps, ModelTimeStep, 0, DisturbanceAmplitude, DisturbanceStartTime, DisturbanceDuration, DisturbanceUpTime));
        case 2
            assignin('base', DisturbanceVariableName, CT_GeneratePulseSignal(SimulationSteps, ModelTimeStep, 0, DisturbanceAmplitude, DisturbanceStartTime, DisturbanceDuration));
        case 3
            assignin('base', DisturbanceVariableName, CT_GenerateStepSignal(SimulationSteps, ModelTimeStep, 0, DisturbanceAmplitude, DisturbanceStartTime));
        case 4
            assignin('base', DisturbanceVariableName, CT_GenerateSineSignal(SimulationSteps, ModelTimeStep, DisturbanceAmplitude));
        case 5
            assignin('base', DisturbanceVariableName, CT_GenerateConstantSignal(SimulationSteps, ModelTimeStep, DisturbanceAmplitude));
    end

    % run the simulation in accelerated mode
    warning('off','all');
    if (AccelerationDisabled)
        simOut = sim(ModelFile, 'SaveOutput','on');
    else
        simOut = sim(ModelFile, 'SimulationMode', 'accelerator', 'SaveOutput','on');
    end
    warning('on','all');
    
    actualValue = simOut.get(ActualVariableName);
            
    % get the starting index for stability, precision and steadiness
    indexStableStart = CT_GetIndexForTimeStep(actualValue.time, (SimulationSteps/2 + 1)*ModelTimeStep + tStable);
    % get the starting index for smoothness-pre&post and responsiveness-pre&post
    indexDistStart = CT_GetIndexForTimeStep(actualValue.time, DisturbanceStartTime);
    indexDistEnd = CT_GetIndexForTimeStep(actualValue.time, DisturbanceStartTime + DisturbanceDuration) + 1;
        
    % calculate the objective functions
    if ObjectiveFunction == 0
        if DisturbanceSignalType > 3
            % constant or sine
            ObjectiveFunctionValues = zeros(7, 1);
            ObjectiveFunctionValues(1) = ObjectiveFunction_Stability(actualValue, indexStableStart);
            ObjectiveFunctionValues(2) = ObjectiveFunction_Precision(actualValue, DesiredValue, indexStableStart);
            ObjectiveFunctionValues(3) = ObjectiveFunction_Smoothness(actualValue, DesiredValue, 1, smoothnessStartDifference);
            ObjectiveFunctionValues(4) = ObjectiveFunction_Responsiveness(actualValue, DesiredValue, 1, responsivenessClose);
            [ObjectiveFunctionValues(5), ObjectiveFunctionValues(6)] = ObjectiveFunction_Steadiness(actualValue, indexStableStart);
            ObjectiveFunctionValues(7) = ObjectiveFunction_PhysicalRange(actualValue, ActualValueRangeStart, ActualValueRangeEnd);
        else
            if DisturbanceSignalType < 3
                ObjectiveFunctionValues = zeros(11, 1);
            else
                ObjectiveFunctionValues = zeros(9, 1);
            end
            ObjectiveFunctionValues(1) = ObjectiveFunction_Stability(actualValue, indexStableStart);
            ObjectiveFunctionValues(2) = ObjectiveFunction_Precision(actualValue, DesiredValue, indexStableStart);
            ObjectiveFunctionValues(3) = ObjectiveFunction_Smoothness(actualValue, DesiredValue, 1, smoothnessStartDifference);
            ObjectiveFunctionValues(4) = ObjectiveFunction_Responsiveness(actualValue, DesiredValue, 1, responsivenessClose);
            [ObjectiveFunctionValues(5), ObjectiveFunctionValues(6)] = ObjectiveFunction_Steadiness(actualValue, indexStableStart);
            ObjectiveFunctionValues(7) = ObjectiveFunction_PhysicalRange(actualValue, ActualValueRangeStart, ActualValueRangeEnd);
            
            % compute the extra objective functions
            actualValueSubRange.signals.values = actualValue.signals.values(1:indexDistStart);
            actualValueSubRange.time = actualValue.time(1:indexDistStart);

            ObjectiveFunctionValues(8) = ObjectiveFunction_Smoothness(actualValueSubRange, DesiredValue, 1, smoothnessStartDifference);
            ObjectiveFunctionValues(9) = ObjectiveFunction_Responsiveness(actualValueSubRange, DesiredValue, 1, responsivenessClose);
            
            % do not add the post-disturbance obj. functions for the step
            if DisturbanceSignalType < 3
                ObjectiveFunctionValues(10) = ObjectiveFunction_Smoothness(actualValue, DesiredValue, indexDistEnd, smoothnessStartDifference);
                ObjectiveFunctionValues(11) = ObjectiveFunction_Responsiveness(actualValue, DesiredValue, indexDistEnd, responsivenessClose);
            end
        end
    else
        switch ObjectiveFunction
            case 1
                ObjectiveFunctionValues = ObjectiveFunction_Stability(actualValue, indexStableStart);
            case 2
                ObjectiveFunctionValues = ObjectiveFunction_Precision(actualValue, DesiredValue, indexStableStart);
            case 3
                ObjectiveFunctionValues = ObjectiveFunction_Smoothness(actualValue, DesiredValue, 1, smoothnessStartDifference);
            case 4                
                ObjectiveFunctionValues = ObjectiveFunction_Responsiveness(actualValue, DesiredValue, 1, responsivenessClose);
            case 5
                ObjectiveFunctionValues = ObjectiveFunction_Steadiness(actualValue, indexStableStart);
            % objective functions below should only be used with the
            % corresponding signal types
            case 6
                actualValueSubRange.signals.values = actualValue.signals.values(1:indexDistStart);
                actualValueSubRange.time = actualValue.time(1:indexDistStart);
                ObjectiveFunctionValues = ObjectiveFunction_Smoothness(actualValueSubRange, DesiredValue, 1, smoothnessStartDifference);
            case 7                
                actualValueSubRange.signals.values = actualValue.signals.values(1:indexDistStart);
                actualValueSubRange.time = actualValue.time(1:indexDistStart);
                ObjectiveFunctionValues = ObjectiveFunction_Responsiveness(actualValueSubRange, DesiredValue, 1, responsivenessClose);
            case 8
                ObjectiveFunctionValues = ObjectiveFunction_Smoothness(actualValue, DesiredValue, indexDistEnd, smoothnessStartDifference);
            case 9
                ObjectiveFunctionValues = ObjectiveFunction_Responsiveness(actualValue, DesiredValue, indexDistEnd, responsivenessClose);
        end
    end
                       
end