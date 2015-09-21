%  RandomExploration_Disturbance
%
%  Executes the model with the disturbance fault model as a basis in an
%  explorative manner, attempting to provide a view of the input space
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
    
    % double the model simulation time since this is the maximum possible
    % total simulation time configurable in the GUI and parallelization
    % requires a preset simulation time
    simulationTime = CT_ModelSimulationTime * 2;
    CT_SetSimulationTime(simulationTime);
    
    % retrieve the model simulation step, as it might have been changed by
    % the configuration script
    CT_ModelTimeStep = CT_GetSimulationTimeStep();
    CT_SimulationSteps=simulationTime/CT_ModelTimeStep;
    
    % compute the total number of regions
    CT_TotalRegions = CT_Regions * CT_Regions;
    
    % build the model if needed
    if (CT_ModelConfigurationFile)
        evalin('base', strcat('run(''',CT_ModelConfigurationFile,''')'));
    end
    assignin('base', CT_DesiredVariableName, CT_GenerateConstantSignal(CT_SimulationSteps, CT_ModelTimeStep, 0));
    assignin('base', CT_DisturbanceVariableName, CT_GenerateConstantSignal(1, CT_SimulationSteps*CT_ModelTimeStep, 0));    
    accelbuild(gcs);
        
    % pre-allocate space
    CT_TotalObjectiveFunctions = 7;
    ObjectiveFunctionValues = zeros(CT_TotalRegions, CT_PointsPerRegion, CT_TotalObjectiveFunctions);
    InputValues = zeros(CT_TotalRegions, CT_PointsPerRegion, 2);
        
    parfor RegionCnt = 1 : CT_TotalRegions
        evalin('base', strcat('run(''',CT_ModelConfigurationFile,''')'));
        if (~strcmp(which(gcs), CT_ModelFile))
            load_system(CT_ModelFile);
            CT_SetSimulationTime(simulationTime);
        end
        
        RegionXIndex = floor((RegionCnt-1) / CT_Regions);
        RegionYIndex = floor(mod(RegionCnt-1, CT_Regions));
        
        RegionXInputValueRangeStart = CT_SineFrequencyRangeStart + ((CT_SineFrequencyRangeEnd - CT_SineFrequencyRangeStart)/CT_Regions) * RegionXIndex;
        RegionXInputValueRangeEnd = CT_SineFrequencyRangeStart + ((CT_SineFrequencyRangeEnd - CT_SineFrequencyRangeStart)/CT_Regions) * (RegionXIndex + 1);
                
        RegionYInputValueRangeStart = CT_DesiredValueRangeStart + ((CT_DesiredValueRangeEnd - CT_DesiredValueRangeStart)/CT_Regions) * RegionYIndex;
        RegionYInputValueRangeEnd = CT_DesiredValueRangeStart + ((CT_DesiredValueRangeEnd - CT_DesiredValueRangeStart)/CT_Regions) * (RegionYIndex + 1);
                
        % generate a random starting point p
        SineFrequency = RegionXInputValueRangeStart + (RegionXInputValueRangeEnd - RegionXInputValueRangeStart) * rand(1);
        DesiredValue = RegionYInputValueRangeStart + (RegionYInputValueRangeEnd - RegionYInputValueRangeStart) * rand(1);
        
        CurrentInputValues = zeros(CT_PointsPerRegion, 2);
        CurrentObjectiveFunctionValues = zeros(CT_PointsPerRegion, CT_TotalObjectiveFunctions);
               
        for PointCnt = 1 : CT_PointsPerRegion
            % save the generated point p
            CurrentInputValues(PointCnt, :) = [SineFrequency, DesiredValue];
            
            CurrentObjectiveFunctionValues(PointCnt, :) = SimulateModelSine(CT_ModelFile, DesiredValue, SineFrequency, CT_SineAmplitude, CT_ActualValueRangeStart, CT_ActualValueRangeEnd, 0, CT_SimulationSteps, CT_ModelTimeStep, CT_DesiredVariableName, CT_ActualVariableName, CT_DisturbanceVariableName, CT_TimeStable, CT_TimeStable, CT_SmoothnessStartDifference, CT_ResponsivenessClose, CT_AccelerationDisabled, CT_ModelConfigurationFile);
            
            % generate a new point p
            if CT_UseAdaptiveRandomSearch == 0
                [SineFrequency, DesiredValue] = RandomExploration_GenerateNew2DPoint(CurrentInputValues, PointCnt, RegionXInputValueRangeStart, RegionXInputValueRangeEnd, RegionYInputValueRangeStart, RegionYInputValueRangeEnd);
            else
                [SineFrequency, DesiredValue] = RandomExploration_GenerateNew2DPointAdaptive(CurrentInputValues, PointCnt, RegionXInputValueRangeStart, RegionXInputValueRangeEnd, RegionYInputValueRangeStart, RegionYInputValueRangeEnd);
            end
        end
        
        % copy the desired values
        InputValues(RegionCnt, :, :) = CurrentInputValues(:, :);
        % copy the objective functions
        ObjectiveFunctionValues(RegionCnt, :, :) = CurrentObjectiveFunctionValues(:, :);
    end  
    
    LimitInputValues = [CT_SineFrequencyRangeStart, CT_DesiredValueRangeStart; CT_SineFrequencyRangeStart, CT_DesiredValueRangeEnd; CT_SineFrequencyRangeEnd, CT_DesiredValueRangeStart; CT_SineFrequencyRangeEnd, CT_DesiredValueRangeEnd];
    LimitObjectiveFunctionValues = zeros(4, CT_TotalObjectiveFunctions);
    
    % temporary variables to avoid communication overhead in parfor loop
    LimitDesiredValuesX = LimitInputValues(:,1);
    LimitDesiredValuesY = LimitInputValues(:,2);
    
    parfor LimitTestCases = 1 : 4
        LimitObjectiveFunctionValues(LimitTestCases, :) = SimulateModelSine(CT_ModelFile, LimitDesiredValuesY(LimitTestCases,1), LimitDesiredValuesX(LimitTestCases,1), CT_SineAmplitude, CT_ActualValueRangeStart, CT_ActualValueRangeEnd, 0, CT_SimulationSteps, CT_ModelTimeStep, CT_DesiredVariableName, CT_ActualVariableName, CT_DisturbanceVariableName, CT_TimeStable, CT_TimeStable, CT_SmoothnessStartDifference, CT_ResponsivenessClose, CT_AccelerationDisabled, CT_ModelConfigurationFile);
    end  
    
    RandomExploration_Sine_SaveResults(InputValues, ObjectiveFunctionValues, LimitInputValues, LimitObjectiveFunctionValues, CT_TotalRegions, CT_PointsPerRegion, CT_TempPath);

    display('Successful termination of the random exploration process.');
catch e
    display('Error during random exploration: ');
    display(getReport(e));
end
