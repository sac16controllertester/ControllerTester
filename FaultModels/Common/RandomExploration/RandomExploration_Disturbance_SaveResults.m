function [ ExplorationResults ] = RandomExploration_Disturbance_SaveResults(InputValues, ObjectiveFunctionValues, LimitInputValues, LimitObjectiveFunctionValues, DisturbanceSignalType, TotalObjectiveFunctions, TotalRegions, PointsPerRegion, TempPath)
    % generates a file containing all the points found and a file
    % containing a processed view of the input space
    ExplorationResults = zeros(PointsPerRegion*TotalRegions+4, TotalObjectiveFunctions+2);
    
    for RegionCnt = 1 : TotalRegions
        for PointCnt = 1 : PointsPerRegion
            ExplorationResults(PointCnt + PointsPerRegion * (RegionCnt-1), 1) = InputValues(RegionCnt, PointCnt, 1);
            ExplorationResults(PointCnt + PointsPerRegion * (RegionCnt-1), 2) = InputValues(RegionCnt, PointCnt, 2);
            for i = 1 : 5
                ExplorationResults(PointCnt + PointsPerRegion * (RegionCnt-1), i+2) = ObjectiveFunctionValues(RegionCnt, PointCnt, i);
            end
            
            for i = 1 : TotalObjectiveFunctions-7 
                ExplorationResults(PointCnt + PointsPerRegion * (RegionCnt-1), i+7) = ObjectiveFunctionValues(RegionCnt, PointCnt, i + 7);
            end
            
            for i = 1 : 2
                ExplorationResults(PointCnt + PointsPerRegion * (RegionCnt-1), i+TotalObjectiveFunctions) = ObjectiveFunctionValues(RegionCnt, PointCnt, i + 5);
            end
        end
    end

    % add limit test cases
    indexStart = PointsPerRegion * TotalRegions + 1;
    for i = indexStart : indexStart + 3
        iLim = i - PointsPerRegion * TotalRegions;
        ExplorationResults(i, 1) = LimitInputValues(iLim, 1);
        ExplorationResults(i, 2) = LimitInputValues(iLim, 2);
        
        for j = 1 : 5
            ExplorationResults(i, j+2) = LimitObjectiveFunctionValues(iLim, j);
        end

        for j = 1 : TotalObjectiveFunctions-7 
            ExplorationResults(i, j+7) = LimitObjectiveFunctionValues(iLim, j+7);
        end

        for j = 1 : 2
            ExplorationResults(i, j+TotalObjectiveFunctions) = LimitObjectiveFunctionValues(iLim, j+5);
        end
    end
    
    
    ResultsFolderPath = strcat(TempPath, '\RandomExplorationDisturbance_', num2str(DisturbanceSignalType));
    if (exist(ResultsFolderPath, 'dir') ~= 7)
        mkdir(ResultsFolderPath);
    end
  
    PointsFilePath = strcat(ResultsFolderPath, '\RandomExplorationPoints.csv');
    if DisturbanceSignalType < 3
        % Trapezoidal Ramp or Pulse
        RandomExplorationResultsHeader={'DisturbanceStartTime,Desired,Stability,Precision,Smoothness,Responsiveness,Steadiness,SmoothnessPre,ResponsivenessPre,SmoothnessPost,ResponsivenessPost,MeanStableValue,PhysicalRangeExceeded'};
    else
        if DisturbanceSignalType == 3
            % Step
            RandomExplorationResultsHeader={'DisturbanceStartTime,Desired,Stability,Precision,Smoothness,Responsiveness,Steadiness,SmoothnessPre,ResponsivenessPre,MeanStableValue,PhysicalRangeExceeded'};
        else
            % Constant or Sine Wave
            RandomExplorationResultsHeader={'DisturbanceStartTime,Desired,Stability,Precision,Smoothness,Responsiveness,Steadiness,MeanStableValue,PhysicalRangeExceeded'};
        end
    end
    dlmwrite(PointsFilePath, RandomExplorationResultsHeader, '');
    dlmwrite(PointsFilePath, ExplorationResults,'-append', 'delimiter', ',', 'newline', 'pc');
  
    RegionsPerAxis = sqrt(TotalRegions);
  
    RegionWorstValues = zeros(RegionsPerAxis, RegionsPerAxis, TotalObjectiveFunctions);
    RegionWorstIndexes = zeros(RegionsPerAxis, RegionsPerAxis, TotalObjectiveFunctions);
    
    RegionMeanValues = zeros(RegionsPerAxis, RegionsPerAxis, TotalObjectiveFunctions);
    RegionIndexes = zeros(RegionsPerAxis, RegionsPerAxis, 2);

    RegionPhysicalRangeExceeded = zeros(RegionsPerAxis, RegionsPerAxis);
    
    for RegionXCnt = 1:RegionsPerAxis
        for RegionYCnt = 1:RegionsPerAxis
            % save the region indexes
            RegionIndexes(RegionXCnt, RegionYCnt, 1) = RegionXCnt;
            RegionIndexes(RegionXCnt, RegionYCnt, 2) = RegionYCnt;
            
            % calculate mean and worst values
            for ObjectiveFncCnt = 1:TotalObjectiveFunctions
                if ObjectiveFncCnt == 6 || ObjectiveFncCnt == 7
                    continue;
                end
                % init worst indexes
                RegionWorstIndexes(RegionXCnt, RegionYCnt, ObjectiveFncCnt, 1) = InputValues(RegionYCnt+(RegionXCnt-1)*RegionsPerAxis, 1, 1);
                RegionWorstIndexes(RegionXCnt, RegionYCnt, ObjectiveFncCnt, 2) = InputValues(RegionYCnt+(RegionXCnt-1)*RegionsPerAxis, 1, 2);
                
                for PointCnt = 1:PointsPerRegion
                    if (ObjectiveFunctionValues(RegionYCnt+(RegionXCnt-1)*RegionsPerAxis, PointCnt, 7) == 1)
                        RegionPhysicalRangeExceeded(RegionXCnt, RegionYCnt) = 1;
                    end
                    val = ObjectiveFunctionValues(RegionYCnt+(RegionXCnt-1)*RegionsPerAxis, PointCnt, ObjectiveFncCnt);
                    RegionMeanValues(RegionXCnt, RegionYCnt, ObjectiveFncCnt) = RegionMeanValues(RegionXCnt, RegionYCnt, ObjectiveFncCnt) + val;
                    if (val > RegionWorstValues(RegionXCnt, RegionYCnt, ObjectiveFncCnt))
                        RegionWorstValues(RegionXCnt, RegionYCnt, ObjectiveFncCnt) = val;
                        RegionWorstIndexes(RegionXCnt, RegionYCnt, ObjectiveFncCnt, 1) = InputValues(RegionYCnt+(RegionXCnt-1)*RegionsPerAxis, PointCnt, 1);
                        RegionWorstIndexes(RegionXCnt, RegionYCnt, ObjectiveFncCnt, 2) = InputValues(RegionYCnt+(RegionXCnt-1)*RegionsPerAxis, PointCnt, 2);
                    end
                end

                limitTestCase = 0;
                % add limit test cases
                if RegionXCnt == 1 && RegionYCnt == 1
                    % bottom left region
                    limitTestCase = 1;
                else
                    if RegionXCnt == 1 && RegionYCnt == RegionsPerAxis
                        % top left region
                        limitTestCase = 2;
                    else
                        if RegionXCnt == RegionsPerAxis && RegionYCnt == 1
                            % bottom right region
                            limitTestCase = 3;
                        else
                            if RegionXCnt == RegionsPerAxis && RegionYCnt == RegionsPerAxis
                                % top right region
                                limitTestCase = 4;
                            end
                        end
                    end
                end
                                
                if limitTestCase == 0
                    % finish calculating the mean
                    RegionMeanValues(RegionXCnt, RegionYCnt, ObjectiveFncCnt) = RegionMeanValues(RegionXCnt, RegionYCnt, ObjectiveFncCnt) / PointsPerRegion;
                else
                    % add limit test cases
                    if (LimitObjectiveFunctionValues(limitTestCase, 7) == 1)
                        RegionPhysicalRangeExceeded(RegionXCnt, RegionYCnt) = 1;
                    end
                    val = LimitObjectiveFunctionValues(limitTestCase, ObjectiveFncCnt);
                    RegionMeanValues(RegionXCnt, RegionYCnt, ObjectiveFncCnt) = RegionMeanValues(RegionXCnt, RegionYCnt, ObjectiveFncCnt) + val;
                    if (val > RegionWorstValues(RegionXCnt, RegionYCnt, ObjectiveFncCnt))
                        RegionWorstValues(RegionXCnt, RegionYCnt, ObjectiveFncCnt) = val;
                        RegionWorstIndexes(RegionXCnt, RegionYCnt, ObjectiveFncCnt, 1) = LimitInputValues(limitTestCase, 1);
                        RegionWorstIndexes(RegionXCnt, RegionYCnt, ObjectiveFncCnt, 2) = LimitInputValues(limitTestCase, 2);
                    end
                    % calculate the mean
                    RegionMeanValues(RegionXCnt, RegionYCnt, ObjectiveFncCnt) = RegionMeanValues(RegionXCnt, RegionYCnt, ObjectiveFncCnt) / (PointsPerRegion + 1);
                end

            end
        end
    end
    
    if DisturbanceSignalType < 3
        RegionResults = zeros(TotalRegions, 39);
    else
        if DisturbanceSignalType == 3
            RegionResults = zeros(TotalRegions, 31);
        else
            RegionResults = zeros(TotalRegions, 23);
        end
    end
    
    for RegionCnt = 1 : TotalRegions        
        RegionXCnt = 1 + floor((RegionCnt-1) / RegionsPerAxis);
        RegionYCnt = 1 + floor(mod(RegionCnt-1, RegionsPerAxis));
        
        RegionResults(RegionCnt, 1) = RegionIndexes(RegionXCnt, RegionYCnt, 1);
        RegionResults(RegionCnt, 2) = RegionIndexes(RegionXCnt, RegionYCnt, 2);
        RegionResults(RegionCnt, 3) = RegionPhysicalRangeExceeded(RegionXCnt, RegionYCnt);
        
        for ObjectiveFncCnt = 1 : 5
            RegionResults(RegionCnt, (ObjectiveFncCnt-1)*4 + 4) = RegionMeanValues(RegionXCnt, RegionYCnt, ObjectiveFncCnt);
            RegionResults(RegionCnt, (ObjectiveFncCnt-1)*4 + 5) = RegionWorstValues(RegionXCnt, RegionYCnt, ObjectiveFncCnt);
            RegionResults(RegionCnt, (ObjectiveFncCnt-1)*4 + 6) = RegionWorstIndexes(RegionXCnt, RegionYCnt, ObjectiveFncCnt, 1);
            RegionResults(RegionCnt, (ObjectiveFncCnt-1)*4 + 7) = RegionWorstIndexes(RegionXCnt, RegionYCnt, ObjectiveFncCnt, 2);
        end
        
        if DisturbanceSignalType < 4
            if DisturbanceSignalType == 3
                TotalExtraObjFun = 2;
            else
                TotalExtraObjFun = 4;
            end
            for ObjectiveFncCnt = 1 : TotalExtraObjFun
                RegionResults(RegionCnt, (ObjectiveFncCnt-1)*4 + 24) = RegionMeanValues(RegionXCnt, RegionYCnt, ObjectiveFncCnt + 7);
                RegionResults(RegionCnt, (ObjectiveFncCnt-1)*4 + 25) = RegionWorstValues(RegionXCnt, RegionYCnt, ObjectiveFncCnt + 7);
                RegionResults(RegionCnt, (ObjectiveFncCnt-1)*4 + 26) = RegionWorstIndexes(RegionXCnt, RegionYCnt, ObjectiveFncCnt + 7, 1);
                RegionResults(RegionCnt, (ObjectiveFncCnt-1)*4 + 27) = RegionWorstIndexes(RegionXCnt, RegionYCnt, ObjectiveFncCnt + 7, 2);
            end
        end
    end
    
    RegionsFilePath = strcat(ResultsFolderPath, '\RandomExplorationRegions.csv');
    if DisturbanceSignalType < 3
        RegionResultsHeader = {'RegionX,RegionY,RangeExceeded,Stability,StabilityWorst,StabilityWorstX,StabilityWorstY,Precision,PrecisionWorst,PrecisionWorstX,PrecisionWorstY,Smoothness,SmoothnessWorst,SmoothnessWorstX,SmoothnessWorstY,Responsiveness,ResponsivenessWorst,ResponsivenessWorstX,ResponsivenessWorstY,Steadiness,SteadinessWorst,SteadinessWorstX,SteadinessWorstY,SmoothnessPre,SmoothnessPreWorst,SmoothnessPreWorstX,SmoothnessPreWorstY,ResponsivenessPre,ResponsivenessPreWorst,ResponsivenessPreWorstX,ResponsivenessPreWorstY,SmoothnessPost,SmoothnessPostWorst,SmoothnessPostWorstX,SmoothnessPostWorstY,ResponsivenessPost,ResponsivenessPostWorst,ResponsivenessPostWorstX,ResponsivenessPostWorstY'};        
    else
        if DisturbanceSignalType == 3
            RegionResultsHeader = {'RegionX,RegionY,RangeExceeded,Stability,StabilityWorst,StabilityWorstX,StabilityWorstY,Precision,PrecisionWorst,PrecisionWorstX,PrecisionWorstY,Smoothness,SmoothnessWorst,SmoothnessWorstX,SmoothnessWorstY,Responsiveness,ResponsivenessWorst,ResponsivenessWorstX,ResponsivenessWorstY,Steadiness,SteadinessWorst,SteadinessWorstX,SteadinessWorstY,SmoothnessPre,SmoothnessPreWorst,SmoothnessPreWorstX,SmoothnessPreWorstY,ResponsivenessPre,ResponsivenessPreWorst,ResponsivenessPreWorstX,ResponsivenessPreWorstY'};        
        else
            RegionResultsHeader = {'RegionX,RegionY,RangeExceeded,Stability,StabilityWorst,StabilityWorstX,StabilityWorstY,Precision,PrecisionWorst,PrecisionWorstX,PrecisionWorstY,Smoothness,SmoothnessWorst,SmoothnessWorstX,SmoothnessWorstY,Responsiveness,ResponsivenessWorst,ResponsivenessWorstX,ResponsivenessWorstY,Steadiness,SteadinessWorst,SteadinessWorstX,SteadinessWorstY'};
        end
    end
                
    dlmwrite(RegionsFilePath, RegionResultsHeader,'');
    dlmwrite(RegionsFilePath, RegionResults,'-append', 'delimiter', ',', 'newline', 'pc');

end

