function [ObjectiveFunctionValues] = SimulateModelAllowedOscillation(ModelFile, InitialDesiredValue, DesiredValue, SimulationSteps, ModelTimeStep, DesiredVariableName, ActualVariableName, tStable, AccelerationDisabled, ModelConfigurationFile, AllowedOscillationPercentage, PlotResults)
    % generate the signal for the desired value  
    if (ModelConfigurationFile)
        evalin('base', strcat('run(''',ModelConfigurationFile,''')'));
    end
    assignin('base', DesiredVariableName, CT_GenerateStepSignal(SimulationSteps, ModelTimeStep, InitialDesiredValue, DesiredValue,SimulationSteps*ModelTimeStep/2));
               
    % run the simulation in accelerated mode
    if (AccelerationDisabled)
        simOut = sim(ModelFile, 'SaveOutput', 'on');
    else
        simOut = sim(ModelFile, 'SimulationMode', 'accelerator', 'SaveOutput', 'on');
    end

    actualValue = simOut.get(ActualVariableName);
    
    if PlotResults == 1
        assignin('base', 'actualValue', simOut.get(ActualVariableName));
        InterpolatedDesiredValues = evalin('base',strcat('interp1(',DesiredVariableName,'.time,',DesiredVariableName,'.signals.values, actualValue.time);'));
        plot(actualValue.time, InterpolatedDesiredValues, actualValue.time, actualValue.signals.values);
    end
    
    indexStableStart = CT_GetIndexForTimeStep(actualValue.time, double((SimulationSteps/2 + 1)*ModelTimeStep) + tStable);
    indexChange = CT_GetIndexForTimeStep(actualValue.time, double((SimulationSteps/2 + 1)*ModelTimeStep));

    % calculate the objective functions
    [ObjectiveFunctionValues(1), ObjectiveFunctionValues(2)] = ObjectiveFunction_NoActualValueChange(actualValue.signals.values, DesiredValue, AllowedOscillationPercentage, indexStableStart, indexChange);
end