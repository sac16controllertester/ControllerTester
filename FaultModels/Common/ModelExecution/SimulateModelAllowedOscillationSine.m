function [ObjectiveFunctionValues] = SimulateModelAllowedOscillationSine(ModelFile, DesiredValue, SineFrequency, SimulationSteps, ModelTimeStep, DesiredVariableName, ActualVariableName, tStable, AccelerationDisabled, ModelConfigurationFile, AllowedOscillationPercentage, PlotResults)
    % generate the time for the desired value  
    if (ModelConfigurationFile)
        evalin('base', strcat('run(''',ModelConfigurationFile,''')'));
    end
    assignin('base', DesiredVariableName, CT_GenerateSineSignal(SimulationSteps, ModelTimeStep, DesiredValue, DesiredValue *  AllowedOscillationPercentage/100, SineFrequency));

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
    
    % the output value is true if the allowed oscillation requirement was
    % respected, false if voided
    % check if the allowed oscillation was exceeded after tStable, assume
    % no (test case passes)
    ObjectiveFunctionValues = true;

    % get the starting index for stability
    indexStableStart = CT_GetIndexForTimeStep(actualValue.time, double((SimulationSteps/2 + 1)*ModelTimeStep) + tStable);
    len = length(actualValue.signals.values);
    
    for i = indexStableStart:len
        if (actualValue.signals.values(i) > DesiredValue * (100 + AllowedOscillationPercentage)/100 || actualValue.signals.values(i) < DesiredValue * (100 - AllowedOscillationPercentage)/100)
            ObjectiveFunctionValues = false;
        end
    end
    
    % check for a standard deviation higher than the
    % allowed oscillation percentage * the desired value
    stdDev = std(actualValue.signals.values(indexStableStart:len));

    if (stdDev > (AllowedOscillationPercentage/2 * DesiredValue) / 100)
        ObjectiveFunctionValues = false;
    end
end