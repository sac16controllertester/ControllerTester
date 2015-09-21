%  SearchSpaceExploration_AllowedOscillation
%
%  Executes the model attempting to void the allowed desired value 
%  oscillation requirement
%
%  Author: Alvin Stanescu
%
try
    % add all the paths containing the model and the functions we use
    addpath(CT_ModelPath);
    addpath(strcat(CT_ScriptsPath, '\ModelExecution'));
    addpath(strcat(CT_ScriptsPath, '\ObjectiveFunctions'));
    addpath(strcat(CT_ScriptsPath, '\Util'));
    % configure the static model configuration parameters and load the
    % model into the system memory
    load_system(CT_ModelFile);
    CT_CheckCorrectOutput(CT_ActualVariableName);
    run(CT_ModelConfigurationFile);
    % double the model simulation time set in the GUI because we need two values for the allowed oscillation fault model
    simulationTime = CT_ModelSimulationTime * 2;
    CT_SetSimulationTime(simulationTime);
    
    % retrieve the model simulation step, as it might have been changed by
    % the configuration script
    CT_ModelTimeStep = CT_GetSimulationTimeStep();
    CT_SimulationSteps=simulationTime/CT_ModelTimeStep;  

    % pre-allocate space
    Passed = zeros(1, 3);    
        
    % build model if needed
    if (CT_ModelConfigurationFile)
        evalin('base', strcat('run(''',CT_ModelConfigurationFile,''')'));
    end

    assignin('base', CT_DesiredVariableName, CT_GenerateStepSignal(CT_SimulationSteps, CT_ModelTimeStep, 0, 0, CT_ModelSimulationTime));
    assignin('base', CT_DisturbanceVariableName, CT_GenerateConstantSignal(1, CT_SimulationSteps*CT_ModelTimeStep, 0));
    accelbuild(gcs);
    
    switch(CT_SelectedTestCase)
        case 1
            FinalDesiredValue = (101 + CT_AllowedOscillationPercentage)/100 * CT_DesiredValue;
            
            % check if input would exceed range => there must be a change in
            % the actual value so that the test case will pass
            if (FinalDesiredValue > CT_DesiredValueRangeEnd)
                FinalDesiredValue = CT_DesiredValueRangeStart;
            end            
        case 2
            FinalDesiredValue = (99 - CT_AllowedOscillationPercentage)/100 * CT_DesiredValue;
            
            % check if input would exceed range => there must be a change in
            % the actual value so that the test case will pass
            if (FinalDesiredValue < CT_DesiredValueRangeStart)
                FinalDesiredValue = CT_DesiredValueRangeEnd;
            end            
        case 3
            FinalDesiredValue = (100 + CT_AllowedOscillationPercentage)/100 * CT_DesiredValue;
            
            % check if input would exceed range => there must be no change in
            % the actual value so that the test case will pass
            if (FinalDesiredValue > CT_DesiredValueRangeEnd)
                FinalDesiredValue = CT_DesiredValue;
            end            
        case 4
            FinalDesiredValue = (100 - CT_AllowedOscillationPercentage)/100 * CT_DesiredValue;
            
            % check if input would exceed range => there must be no change in
            % the actual value so that the test case will pass
            if (FinalDesiredValue < CT_DesiredValueRangeStart)
                FinalDesiredValue = CT_DesiredValue;
            end            
    end
        
    if (CT_SelectedTestCase < 5)
        Passed = SimulateModelAllowedOscillation(CT_ModelFile, CT_DesiredValue, FinalDesiredValue, CT_SimulationSteps, CT_ModelTimeStep, CT_DesiredVariableName, CT_ActualVariableName, CT_TimeStable, CT_AccelerationDisabled, CT_ModelConfigurationFile, CT_AllowedOscillationPercentage, 1);
        if (CT_SelectedTestCase < 3)
            Passed = ~Passed;
        end
    else
        if (CT_SelectedTestCase == 5)
            Passed = SimulateModelAllowedOscillationSine(CT_ModelFile, CT_DesiredValue, CT_SineFrequency, CT_SimulationSteps, CT_ModelTimeStep, CT_DesiredVariableName, CT_ActualVariableName, CT_TimeStable, CT_AccelerationDisabled, CT_ModelConfigurationFile, CT_AllowedOscillationPercentage, 1);
        else
            Passed = ~SimulateModelAllowedOscillationSine(CT_ModelFile, CT_DesiredValue, CT_SineFrequency, CT_SimulationSteps, CT_ModelTimeStep, CT_DesiredVariableName, CT_ActualVariableName, CT_TimeStable, CT_AccelerationDisabled, CT_ModelConfigurationFile, 2 * CT_AllowedOscillationPercentage, 1);            
        end
    end     
        
    display(strcat('passed=',num2str(Passed)));
    display('Successful termination of the search space exploration process.');
catch e
    display('An error has occurred during the search space exploration: ');
    display(getReport(e));
end
