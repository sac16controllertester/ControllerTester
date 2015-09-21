%  SearchSpaceExploration_AllowedOscillation
%
%  Executes the model with the step fault model as a basis attempting to
%  void the allowed desired value oscillation requirement
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
    
    % compute the number of test desired values
    CT_TotalDesiredValues = (CT_DesiredValueRangeEnd - CT_DesiredValueRangeStart) / CT_DesiredValueStepSize;
       
    % build the model if needed
    if (CT_ModelConfigurationFile)
        evalin('base', strcat('run(''',CT_ModelConfigurationFile,''')'));
    end
    assignin('base', CT_DesiredVariableName, CT_GenerateStepSignal(CT_SimulationSteps, CT_ModelTimeStep, 0, 0, CT_ModelSimulationTime));
    assignin('base', CT_DisturbanceVariableName, CT_GenerateConstantSignal(1, CT_SimulationSteps*CT_ModelTimeStep, 0));
    
    accelbuild(gcs);       

    % pre-allocate space
    StepTestCases = zeros(CT_TotalDesiredValues, 4, 4);    
    if (CT_GenerateSineWaveTestCases == true)
        SineTestCases = zeros(CT_TotalDesiredValues, 2, 2);
    end

    parfor DesiredValueCnt = 1 : CT_TotalDesiredValues
        evalin('base', strcat('run(''',CT_ModelConfigurationFile,''')'));
        
        assignin('base', CT_DisturbanceVariableName, CT_GenerateConstantSignal(1, CT_SimulationSteps*CT_ModelTimeStep, 0));

        IgnoreTC = zeros(4,1);
        if (~strcmp(which(gcs), CT_ModelFile))
            load_system(CT_ModelFile);
            CT_SetSimulationTime(simulationTime);
        end

        % generate input points for the step test cases
        InitialDesiredValue = CT_DesiredValueRangeStart + DesiredValueCnt*CT_DesiredValueStepSize;
        CurrentStepValues = zeros(4, 4);
        CurrentStepValues(:,1) = [InitialDesiredValue, InitialDesiredValue, InitialDesiredValue, InitialDesiredValue];
        CurrentStepValues(:,2) = [(101 + CT_AllowedOscillationPercentage)/100 * InitialDesiredValue, (99 - CT_AllowedOscillationPercentage)/100 * InitialDesiredValue, (100 + CT_AllowedOscillationPercentage)/100 * InitialDesiredValue, (100 - CT_AllowedOscillationPercentage)/100 * InitialDesiredValue];
        
        % check if input would exceed range => there must be a change in
        % the actual value so that the test case will pass
        for i = 1 : 4
            if (CurrentStepValues(i,2) > CT_DesiredValueRangeEnd || CurrentStepValues(i,2) < CT_DesiredValueRangeStart)
                IgnoreTC(i) = 1;
            end
        end    
        
        for TestCaseCnt = 1 : 4
            if ~IgnoreTC(TestCaseCnt)
                out = SimulateModelAllowedOscillation(CT_ModelFile, InitialDesiredValue, CurrentStepValues(TestCaseCnt, 2), CT_SimulationSteps, CT_ModelTimeStep, CT_DesiredVariableName, CT_ActualVariableName, CT_TimeStable, CT_AccelerationDisabled, CT_ModelConfigurationFile, CT_AllowedOscillationPercentage, 0);
            else
                out = [NaN NaN];
            end
            CurrentStepValues(TestCaseCnt, 3) = out(1);
            CurrentStepValues(TestCaseCnt, 4) = out(2);
        end

        % copy the test case results
        StepTestCases(DesiredValueCnt, :) = CurrentStepValues(:);

        % generate sine wave test cases if enabled
        if (CT_GenerateSineWaveTestCases == true)
            CurrentSineValues = zeros(2, 2);
            CurrentSineValues(:,1) = [InitialDesiredValue, InitialDesiredValue];
            if (InitialDesiredValue * (100 - CT_AllowedOscillationPercentage)/100 >= CT_DesiredValueRangeStart && InitialDesiredValue * (100 + CT_AllowedOscillationPercentage)/100 <= CT_DesiredValueRangeEnd)
                % the first test case should not report a problem
                CurrentSineValues(1,2) = SimulateModelAllowedOscillationSine(CT_ModelFile, InitialDesiredValue, CT_SineFrequency, CT_SimulationSteps, CT_ModelTimeStep, CT_DesiredVariableName, CT_ActualVariableName, CT_TimeStable, CT_AccelerationDisabled, CT_ModelConfigurationFile, CT_AllowedOscillationPercentage, 0);
            else
                CurrentSineValues(1,2) = NaN;
            end
            if (InitialDesiredValue * (100 - 2*CT_AllowedOscillationPercentage)/100 >= CT_DesiredValueRangeStart && InitialDesiredValue * (100 + 2*CT_AllowedOscillationPercentage)/100 <= CT_DesiredValueRangeEnd)
                % the second test case should report a problem
                CurrentSineValues(2,2) = ~SimulateModelAllowedOscillationSine(CT_ModelFile, InitialDesiredValue, CT_SineFrequency, CT_SimulationSteps, CT_ModelTimeStep, CT_DesiredVariableName, CT_ActualVariableName, CT_TimeStable, CT_AccelerationDisabled, CT_ModelConfigurationFile, 2 * CT_AllowedOscillationPercentage, 0);
            else
                CurrentSineValues(2,2) = NaN;
            end
            SineTestCases(DesiredValueCnt, :) = CurrentSineValues(:);
        end     
 
    end  
    
    AllowedOscillation_SaveStepResults(StepTestCases, CT_TotalDesiredValues, CT_TempPath);
    
    if (CT_GenerateSineWaveTestCases == true)
        AllowedOscillation_SaveSineResults(SineTestCases, CT_TotalDesiredValues, CT_TempPath);
    end
    
    
    display('Successful termination of the search space exploration process.');
catch e
    display('An error has occurred during the search space exploration: ');
    display(getReport(e));
end
