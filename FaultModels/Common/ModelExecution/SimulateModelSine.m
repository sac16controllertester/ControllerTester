function [ObjectiveFunctionValues] = SimulateModelSine(ModelFile, DesiredValue, SineFrequency, SineAmplitude, ActualValueRangeStart, ActualValueRangeEnd, ObjectiveFunction, SimulationSteps, ModelTimeStep, DesiredVariableName, ActualVariableName, DisturbanceVariableName, tStable, tLive, smoothnessStartDifference, responsivenessClose, AccelerationDisabled, ModelConfigurationFile)
    % re-configure the model
    if (ModelConfigurationFile)
        evalin('base', strcat('run(''',ModelConfigurationFile,''')'));
    end
    
    % generate the time for the desired value  
    assignin('base', DesiredVariableName, CT_GenerateSineSignal(SimulationSteps, ModelTimeStep, DesiredValue, SineAmplitude, SineFrequency));
    assignin('base', DisturbanceVariableName, CT_GenerateConstantSignal(1, SimulationSteps*ModelTimeStep, 0));

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
    indexStableStart = CT_GetIndexForTimeStep(actualValue.time, double((SimulationSteps/2 + 1)*ModelTimeStep) + tStable);
        
    % calculate the objective functions
    if ObjectiveFunction == 0
        ObjectiveFunctionValues = zeros(7, 1);
        ObjectiveFunctionValues(1) = ObjectiveFunction_Stability(actualValue, indexStableStart);
        ObjectiveFunctionValues(2) = ObjectiveFunction_Precision(actualValue, DesiredValue, indexStableStart);
        ObjectiveFunctionValues(3) = ObjectiveFunction_Smoothness(actualValue, DesiredValue, 1, smoothnessStartDifference);
        ObjectiveFunctionValues(4) = ObjectiveFunction_Responsiveness(actualValue, DesiredValue, 1, responsivenessClose);
        [ObjectiveFunctionValues(5), ObjectiveFunctionValues(6)] = ObjectiveFunction_Steadiness(actualValue, indexStableStart);
        ObjectiveFunctionValues(7) = ObjectiveFunction_PhysicalRange(actualValue, ActualValueRangeStart, ActualValueRangeEnd);
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
        end
    end
                       
end