function AllowedOscillation_SaveSineResults(SearchSpaceExplorationResults, TotalDesiredValues, TempPath)
    % generates a file containing all the points found and a file
    % containing a processed view of the input space
    ExplorationResults = zeros(TotalDesiredValues,3);
    for PointCnt = 1 : TotalDesiredValues
        ExplorationResults(PointCnt, 1) = SearchSpaceExplorationResults(PointCnt, 1, 1);
        ExplorationResults(PointCnt, 2) = SearchSpaceExplorationResults(PointCnt, 1, 2);
        ExplorationResults(PointCnt, 3) = SearchSpaceExplorationResults(PointCnt, 2, 2);      
    end
    
    
    ResultsFolderPath = strcat(TempPath, '\AllowedOscillation');
    if (exist(ResultsFolderPath, 'dir') ~= 7)
        mkdir(ResultsFolderPath);
    end
  
    PointsFilePath = strcat(ResultsFolderPath, '\AllowedOscillation_SineResults.csv');
    ExplorationResultsHeader={'InitialDesired,PassedTestCase1,PassedTestCase2'};
    dlmwrite(PointsFilePath, ExplorationResultsHeader, '');
    dlmwrite(PointsFilePath, ExplorationResults,'-append', 'delimiter', ',', 'newline', 'pc');
  
end

