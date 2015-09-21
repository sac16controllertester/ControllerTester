function AllowedOscillation_SaveStepResults(SearchSpaceExplorationResults, TotalDesiredValues, TempPath)
    % generates a file containing all the points found and a file
    % containing a processed view of the input space
    ExplorationResults = zeros(TotalDesiredValues,13);
    
    for PointCnt = 1 : TotalDesiredValues
        ExplorationResults(PointCnt, 1) = SearchSpaceExplorationResults(PointCnt, 1, 1);
        ExplorationResults(PointCnt, 2) = SearchSpaceExplorationResults(PointCnt, 1, 2);
        ExplorationResults(PointCnt, 3) = SearchSpaceExplorationResults(PointCnt, 2, 2);
        ExplorationResults(PointCnt, 4) = SearchSpaceExplorationResults(PointCnt, 3, 2);
        ExplorationResults(PointCnt, 5) = SearchSpaceExplorationResults(PointCnt, 4, 2);
        
        % pass condition is a change in actual value for the first two test
        % cases, therefore we negate the objective function value
        % (NoActualValueChange)
        if (~isnan(SearchSpaceExplorationResults(PointCnt, 1, 3)))
            ExplorationResults(PointCnt, 6) = ~SearchSpaceExplorationResults(PointCnt, 1, 3);
        else
            ExplorationResults(PointCnt, 6) = NaN;
        end
        
        if (~isnan(SearchSpaceExplorationResults(PointCnt, 2, 3)))
            ExplorationResults(PointCnt, 7) = ~SearchSpaceExplorationResults(PointCnt, 2, 3);
        else
            ExplorationResults(PointCnt, 7) = NaN;
        end
        
        ExplorationResults(PointCnt, 8) = SearchSpaceExplorationResults(PointCnt, 3, 3);
        ExplorationResults(PointCnt, 9) = SearchSpaceExplorationResults(PointCnt, 4, 3);
        
        % possible stability problems
        ExplorationResults(PointCnt, 10) = SearchSpaceExplorationResults(PointCnt, 1, 4);
        ExplorationResults(PointCnt, 11) = SearchSpaceExplorationResults(PointCnt, 2, 4);
        ExplorationResults(PointCnt, 12) = SearchSpaceExplorationResults(PointCnt, 3, 4);
        ExplorationResults(PointCnt, 13) = SearchSpaceExplorationResults(PointCnt, 4, 4);
    end
    
    
    ResultsFolderPath = strcat(TempPath, '\AllowedOscillation');
    if (exist(ResultsFolderPath, 'dir') ~= 7)
        mkdir(ResultsFolderPath);
    end
  
    PointsFilePath = strcat(ResultsFolderPath, '\AllowedOscillation_StepResults.csv');
    ExplorationResultsHeader={'InitialDesired,DesiredValueTestCase1,DesiredValueTestCase2,DesiredValueTestCase3,DesiredValueTestCase4,PassedTestCase1,PassedTestCase2,PassedTestCase3,PassedTestCase4,StabilitySmell1,StabilitySmell2,StabilitySmell3,StabilitySmell4'};
    dlmwrite(PointsFilePath, ExplorationResultsHeader, '');
    dlmwrite(PointsFilePath, ExplorationResults,'-append', 'delimiter', ',', 'newline', 'pc');
  
end

