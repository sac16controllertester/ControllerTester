% this script executes the model with the step fault model as a basis
try
    % add all the paths containing the model and the functions we use
    addpath(CT_ModelPath);
    addpath(strcat(CT_ScriptsPath, '/ObjectiveFunctions'));
    addpath(strcat(CT_ScriptsPath, '/Util'));
    % configure the static model configuration parameters and load the
    % model into the system memory
    load_system(CT_ModelFile);    
    CT_CheckCorrectOutput(CT_ActualVariableName);
    run(CT_ModelConfigurationFile);

    % double the model simulation time set in the GUI because we need two values for the step fault model
    simulationTime = CT_ModelSimulationTime * 2;
    CT_SetSimulationTime(simulationTime);
    % retrieve the model simulation step, as it might have been changed by
    % the configuration script
    CT_ModelTimeStep = CT_GetSimulationTimeStep();
    CT_SimulationSteps=simulationTime/CT_ModelTimeStep;
    
    % pre-allocate space
	ObjectiveFunctionValues = zeros(9,3);

    % start the timer to measure the running time of the model together
    % with the objective function computation
    tic;
    % generate the signal for the desired value  
    assignin('base', CT_DesiredVariableName, CT_GenerateStepSignal(CT_SimulationSteps, CT_ModelTimeStep, CT_InitialDesiredValue, CT_DesiredValue, CT_ModelSimulationTime));
    assignin('base', CT_DisturbanceVariableName, CT_GenerateConstantSignal(1, CT_SimulationSteps*CT_ModelTimeStep, 0));
    assignin('base', CT_Compared_DisturbanceVariableName, CT_GenerateConstantSignal(1, CT_SimulationSteps*CT_ModelTimeStep, 0));
    accelbuild(gcs);   
    
    load_system(CT_Compared_ModelFile);  
    CT_CheckCorrectOutput(CT_Compared_ActualVariableName);
    CT_SetSimulationTime(simulationTime);
    run(CT_Compared_ModelConfigurationFile);
    if (strcmp(CT_DesiredVariableName,CT_Compared_DesiredVariableName))
        assignin('base', CT_Compared_DesiredVariableName, CT_GenerateStepSignal(CT_SimulationSteps, CT_ModelTimeStep, CT_InitialDesiredValue, CT_DesiredValue, CT_ModelSimulationTime));
    end
    accelbuild(gcs);
    
    % run the simulation in accelerated mode
    warning('off','all');
    if (CT_AccelerationDisabled)
        if (CT_ModelConfigurationFile)
            evalin('base', strcat('run(''',CT_ModelConfigurationFile,''')'));
        end
        simOut = sim(CT_ModelFile, 'SaveOutput', 'on');
       
        if (CT_Compared_ModelConfigurationFile)
            evalin('base', strcat('run(''',CT_Compared_ModelConfigurationFile,''')'));
        end
        simOut2 = sim(CT_Compared_ModelFile, 'SaveOutput', 'on');
    else
        
        if (CT_ModelConfigurationFile)
            evalin('base', strcat('run(''',CT_ModelConfigurationFile,''')'));
        end
        simOut = sim(CT_ModelFile, 'SimulationMode', 'accelerator', 'SaveOutput', 'on');
        
        if (CT_Compared_ModelConfigurationFile)
            evalin('base', strcat('run(''',CT_Compared_ModelConfigurationFile,''')'));
        end
        simOut2 = sim(CT_Compared_ModelFile, 'SimulationMode', 'accelerator', 'SaveOutput', 'on');
    end
    warning('on','all');
    
    actualValue = simOut.get(CT_ActualVariableName);
    actualValue2 = simOut2.get(CT_Compared_ActualVariableName);

    % get the starting index for stability, precision and steadiness
    indexStableStart = CT_GetIndexForTimeStep(actualValue.time, (CT_SimulationSteps/2+1)*CT_ModelTimeStep + CT_TimeStable);
    indexStableStart2 = CT_GetIndexForTimeStep(actualValue2.time, (CT_SimulationSteps/2+1)*CT_ModelTimeStep + CT_TimeStable);
    % get the starting index for smoothness and responsiveness
    indexMidStart = CT_GetIndexForTimeStep(actualValue.time, (CT_SimulationSteps/2+1)*CT_ModelTimeStep);
    indexMidStart2 = CT_GetIndexForTimeStep(actualValue2.time, (CT_SimulationSteps/2+1)*CT_ModelTimeStep);

    % calculate the objective functions - controller 1
    ObjectiveFunctionValues(1,1) = ObjectiveFunction_Stability(actualValue, indexStableStart);
    ObjectiveFunctionValues(2,1) = ObjectiveFunction_Precision(actualValue, CT_DesiredValue, indexStableStart);
    ObjectiveFunctionValues(3,1) = ObjectiveFunction_Smoothness(actualValue, CT_DesiredValue, indexMidStart, CT_SmoothnessStartDifference);
    ObjectiveFunctionValues(4,1) = ObjectiveFunction_Responsiveness(actualValue, CT_DesiredValue, indexMidStart, CT_ResponsivenessClose);
    [ObjectiveFunctionValues(5,1), ObjectiveFunctionValues(6,1)] = ObjectiveFunction_Steadiness(actualValue, indexStableStart);
    ObjectiveFunctionValues(7,1) = ObjectiveFunction_PhysicalRange(actualValue, CT_ActualValueRangeStart, CT_ActualValueRangeEnd);

    % calculate the objective functions - controller 2
    ObjectiveFunctionValues(1,2) = ObjectiveFunction_Stability(actualValue2, indexStableStart2);
    ObjectiveFunctionValues(2,2) = ObjectiveFunction_Precision(actualValue2, CT_DesiredValue, indexStableStart2);
    ObjectiveFunctionValues(3,2) = ObjectiveFunction_Smoothness(actualValue2, CT_DesiredValue, indexMidStart2, CT_SmoothnessStartDifference);
    ObjectiveFunctionValues(4,2) = ObjectiveFunction_Responsiveness(actualValue2, CT_DesiredValue, indexMidStart2, CT_ResponsivenessClose);
    [ObjectiveFunctionValues(5,2), ObjectiveFunctionValues(6,2)] = ObjectiveFunction_Steadiness(actualValue2, indexStableStart2);
    ObjectiveFunctionValues(7,2) = ObjectiveFunction_PhysicalRange(actualValue2, CT_ActualValueRangeStart, CT_ActualValueRangeEnd);
    
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
    ObjectiveFunctionValues(6, 3) = ObjectiveFunctionCompare_MeanDeviation(actualValue.signals.values, InterpolatedController2Values, CT_DesiredValue);
    ObjectiveFunctionValues(7, 3) = ObjectiveFunctionCompare_MaxDeviation(actualValue.signals.values, InterpolatedController2Values, CT_DesiredValue);
    ObjectiveFunctionValues(8, 3) = ObjectiveFunctionValues(7, 1);
    ObjectiveFunctionValues(9, 3) = ObjectiveFunctionValues(7, 2);

    % stop the timer
    duration = toc;
    % output the model running time (?)
    display('Successful execution of the model');
    display(strcat('runningTime=', num2str(duration)));
    
    str = {'Objective function values (comparison):'};
    str = [str, strcat('Stability: ', num2str(ObjectiveFunctionValues(1, 3)))];
    str = [str, strcat('Precision: ', num2str(ObjectiveFunctionValues(2, 3)))];
    str = [str, strcat('Smoothness: ', num2str(ObjectiveFunctionValues(3, 3)))];
    str = [str, strcat('Responsiveness: ', num2str(ObjectiveFunctionValues(4, 3)))];
    str = [str, strcat('Steadiness: ', num2str(ObjectiveFunctionValues(5, 3)))];
    str = [str, strcat('Mean deviation: ', num2str(ObjectiveFunctionValues(6, 3)))];
    str = [str, strcat('Max deviation: ', num2str(ObjectiveFunctionValues(7, 3)))];

    % plot the result (difference)
    eval(strcat('InterpolatedDesiredValues = interp1(',CT_DesiredVariableName,'.time,',CT_DesiredVariableName,'.signals.values, actualValue.time);'));
    plot(actualValue.time, InterpolatedDesiredValues, actualValue.time, actualValue.signals.values, actualValue.time, InterpolatedController2Values);

    eval(strcat('plot(', CT_DesiredVariableName,'.time,', CT_DesiredVariableName, '.signals.values,', CT_DesiredVariableName, '.time, actualValue.signals.values,', CT_DesiredVariableName, '.time, actualValue2.signals.values,', CT_DesiredVariableName, '.time, actualValueDiff)'));

    annotation('textbox', [0, 0.5, 0, 0], 'string', str);
    
    legend('Desired Value','Actual Value (Project Controller)', 'Actual Value (Compared Controller)', 'Actual Value (Difference)');

catch e
    display('Error during model execution');
    display(getReport(e));
end
