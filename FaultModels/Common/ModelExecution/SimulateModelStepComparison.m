function [ObjectiveFunctionValues] = SimulateModelStepComparison(ModelFile, ModelFile2, InitialDesiredValue, DesiredValue, ActualValueRangeStart, ActualValueRangeEnd, ObjectiveFunction, SimulationSteps, ModelTimeStep, DesiredVariableName, DesiredVariableName2, ActualVariableName, ActualVariableName2, DisturbanceVariableName, DisturbanceVariableName2, tStable, tLive, smoothnessStartDifference, responsivenessClose, AccelerationDisabled, ModelConfigurationFile, ModelConfigurationFile2)
    % generate the signal for the desired value     
    assignin('base', DesiredVariableName, CT_GenerateStepSignal(SimulationSteps, ModelTimeStep, InitialDesiredValue, DesiredValue, SimulationSteps/2*ModelTimeStep));
   
    if (strcmp(DesiredVariableName,DesiredVariableName2))
        assignin('base', DesiredVariableName2, CT_GenerateStepSignal(SimulationSteps, ModelTimeStep, InitialDesiredValue, DesiredValue, SimulationSteps/2*ModelTimeStep));
    end
    assignin('base', DisturbanceVariableName, CT_GenerateConstantSignal(1, SimulationSteps*ModelTimeStep, 0));
    assignin('base', DisturbanceVariableName2, CT_GenerateConstantSignal(1, SimulationSteps*ModelTimeStep, 0));
        
    % run the simulation in accelerated mode
    warning('off','all');
    if (AccelerationDisabled)
        if (~strcmp(which(gcs), ModelFile))
            load_system(ModelFile);
            CT_SetSimulationTime(SimulationSteps*ModelTimeStep);
        end

        if (ModelConfigurationFile)
            evalin('base', strcat('run(''',ModelConfigurationFile,''')'));
        end
        simOut = sim(ModelFile, 'SaveOutput', 'on');
           
        if (~strcmp(which(gcs), ModelFile2))
            load_system(ModelFile2);
            CT_SetSimulationTime(SimulationSteps*ModelTimeStep);
        end

        if (ModelConfigurationFile2)
            evalin('base', strcat('run(''',ModelConfigurationFile2,''')'));
        end
        simOut2 = sim(ModelFile2, 'SaveOutput', 'on');
    else
        if (~strcmp(which(gcs), ModelFile))
            load_system(ModelFile);
            CT_SetSimulationTime(SimulationSteps*ModelTimeStep);
        end

        if (ModelConfigurationFile)
            evalin('base', strcat('run(''',ModelConfigurationFile,''')'));
        end
        simOut = sim(ModelFile, 'SimulationMode', 'accelerator', 'SaveOutput', 'on');
        
        if (~strcmp(which(gcs), ModelFile2))
            load_system(ModelFile2);
            CT_SetSimulationTime(SimulationSteps*ModelTimeStep);
        end
        
        if (ModelConfigurationFile2)
            evalin('base', strcat('run(''',ModelConfigurationFile2,''')'));
        end
        simOut2 = sim(ModelFile2, 'SimulationMode', 'accelerator', 'SaveOutput', 'on');
    end
    warning('on','all');
    
    actualValue = simOut.get(ActualVariableName);
    actualValue2 = simOut2.get(ActualVariableName2);
            
    % get the starting index for stability, precision and steadiness
    indexStableStart = CT_GetIndexForTimeStep(actualValue.time, (SimulationSteps/2 + 1)*ModelTimeStep + tStable);
    indexStableStart2 = CT_GetIndexForTimeStep(actualValue2.time, (SimulationSteps/2 + 1)*ModelTimeStep + tStable);
    % get the starting index for smoothness and responsiveness
    indexMidStart = CT_GetIndexForTimeStep(actualValue.time, (SimulationSteps/2 + 1)*ModelTimeStep);
    indexMidStart2 = CT_GetIndexForTimeStep(actualValue2.time, (SimulationSteps/2 + 1)*ModelTimeStep);

    % calculate the objective functions
    if ObjectiveFunction == 0
        ObjectiveFunctionValues = zeros(9, 3);
        
        % compute the objective functions for model 1
        ObjectiveFunctionValues(1,1) = ObjectiveFunction_Stability(actualValue, indexStableStart);
        ObjectiveFunctionValues(2,1) = ObjectiveFunction_Precision(actualValue, DesiredValue, indexStableStart);
        ObjectiveFunctionValues(3,1) = ObjectiveFunction_Smoothness(actualValue, DesiredValue, indexMidStart, smoothnessStartDifference);
        ObjectiveFunctionValues(4,1) = ObjectiveFunction_Responsiveness(actualValue, DesiredValue, indexMidStart, responsivenessClose);
        [ObjectiveFunctionValues(5,1), ObjectiveFunctionValues(6,1)] = ObjectiveFunction_Steadiness(actualValue, indexStableStart);
        ObjectiveFunctionValues(7,1) = ObjectiveFunction_PhysicalRange(actualValue, ActualValueRangeStart, ActualValueRangeEnd);

        % compute the objective functions for model 2
        ObjectiveFunctionValues(1,2) = ObjectiveFunction_Stability(actualValue2, indexStableStart2);
        ObjectiveFunctionValues(2,2) = ObjectiveFunction_Precision(actualValue2, DesiredValue, indexStableStart2);
        ObjectiveFunctionValues(3,2) = ObjectiveFunction_Smoothness(actualValue2, DesiredValue, indexMidStart2, smoothnessStartDifference);
        ObjectiveFunctionValues(4,2) = ObjectiveFunction_Responsiveness(actualValue2, DesiredValue, indexMidStart2, responsivenessClose);
        [ObjectiveFunctionValues(5,2), ObjectiveFunctionValues(6,2)] = ObjectiveFunction_Steadiness(actualValue2, indexStableStart2);
        ObjectiveFunctionValues(7,2) = ObjectiveFunction_PhysicalRange(actualValue2, ActualValueRangeStart, ActualValueRangeEnd);
        
        % compute the comparison objective functions
        ObjectiveFunctionValues(1, 3) = abs(ObjectiveFunctionValues(1, 2) - ObjectiveFunctionValues(1, 1));
        ObjectiveFunctionValues(2, 3) = abs(ObjectiveFunctionValues(2, 2) - ObjectiveFunctionValues(2, 1));
        if ObjectiveFunctionValues(3,1) ~= Inf && ObjectiveFunctionValues(3,2) ~= Inf
            ObjectiveFunctionValues(3, 3) = abs(ObjectiveFunctionValues(3, 2) - ObjectiveFunctionValues(3, 1));
        else
            ObjectiveFunctionValues(3, 3) = Inf;
        end
        ObjectiveFunctionValues(4, 3) = abs(ObjectiveFunctionValues(4, 2) - ObjectiveFunctionValues(4, 1));
        ObjectiveFunctionValues(5, 3) = abs(ObjectiveFunctionValues(5, 2) - ObjectiveFunctionValues(5, 1));
        InterpolatedController2Values = interp1(actualValue2.time, actualValue2.signals.values, actualValue.time);
        ObjectiveFunctionValues(6, 3) = ObjectiveFunctionCompare_MeanDeviation(actualValue.signals.values, InterpolatedController2Values, DesiredValue);
        ObjectiveFunctionValues(7, 3) = ObjectiveFunctionCompare_MaxDeviation(actualValue.signals.values, InterpolatedController2Values, DesiredValue);
        ObjectiveFunctionValues(8, 3) = ObjectiveFunctionValues(7, 1);
        ObjectiveFunctionValues(9, 3) = ObjectiveFunctionValues(7, 2);
    else
        switch ObjectiveFunction
            case 1
                ObjectiveFunctionValues = abs(ObjectiveFunction_Stability(actualValue, indexStableStart) - ObjectiveFunction_Stability(actualValue2, indexStableStart2));
            case 2
                ObjectiveFunctionValues = abs(ObjectiveFunction_Precision(actualValue, DesiredValue, indexStableStart) - ObjectiveFunction_Precision(actualValue2, DesiredValue, indexStableStart2));
            case 3
                ObjectiveFunctionValues = abs(ObjectiveFunction_Smoothness(actualValue, DesiredValue, indexMidStart, smoothnessStartDifference) - ObjectiveFunction_Smoothness(actualValue2, DesiredValue, indexMidStart2, smoothnessStartDifference));
            case 4
                ObjectiveFunctionValues = abs(ObjectiveFunction_Responsiveness(actualValue, DesiredValue, indexMidStart, responsivenessClose) - ObjectiveFunction_Responsiveness(actualValue2, DesiredValue, indexMidStart2, responsivenessClose));
            case 5
                ObjectiveFunctionValues = abs(ObjectiveFunction_Steadiness(actualValue, indexStableStart) - ObjectiveFunction_Steadiness(actualValue2, indexStableStart2));
            case 6
                InterpolatedController2Values = interp1(actualValue2.time, actualValue2.signals.values, actualValue.time);
                ObjectiveFunctionValues = ObjectiveFunctionCompare_MeanDeviation(actualValue.signals.values, InterpolatedController2Values, DesiredValue);
            case 7
                InterpolatedController2Values = interp1(actualValue2.time, actualValue2.signals.values, actualValue.time);
                ObjectiveFunctionValues = ObjectiveFunctionCompare_MaxDeviation(actualValue.signals.values, InterpolatedController2Values, DesiredValue);
        end
    end    
end