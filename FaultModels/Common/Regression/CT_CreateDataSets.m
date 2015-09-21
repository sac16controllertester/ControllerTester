function [TrainingSet, TestSet, worstObjectiveFunction] = CT_CreateDataSets(hSimulation, TrainingSetSizeEqualDistance, TrainingSetSizeRandom, ValidationSetSize, RegionXStart, RegionXEnd, RegionYStart, RegionYEnd, RefinedCandidatePoints, RefinementPoints, StartPoint)
    [TrainingSetEqual.Input, TrainingSetEqual.Output, TrainingSetEqual.Total] = CT_CreateSimulationSamples2D(TrainingSetSizeEqualDistance, hSimulation, RegionXStart, RegionXEnd, RegionYStart, RegionYEnd, false);
    [TrainingSetRandom.Input, TrainingSetRandom.Output, TrainingSetRandom.Total] = CT_CreateSimulationSamples2D(TrainingSetSizeRandom, hSimulation, RegionXStart, RegionXEnd, RegionYStart, RegionYEnd, true);
        
    TrainingSet.Input = vertcat(TrainingSetEqual.Input, TrainingSetRandom.Input, StartPoint);
    TrainingSet.Output = vertcat(TrainingSetEqual.Output, TrainingSetRandom.Output, hSimulation(StartPoint));
    TrainingSet.Total = TrainingSetEqual.Total + TrainingSetRandom.Total + 1;
        
    if RefinedCandidatePoints == 0
        worstObjectiveFunction = CT_FindWorstObjectiveFunctions(TrainingSet, 1);
    else
        worstObjectiveFunctions = CT_FindWorstObjectiveFunctions(TrainingSet, RefinedCandidatePoints);
        
        for i = 1 : RefinedCandidatePoints
            SampleInput = zeros(RefinementPoints,2);
            SampleOutput = zeros(RefinementPoints,1);

            [RefinedRegionXStart, RefinedRegionXEnd, RefinedRegionYStart, RefinedRegionYEnd] = CT_FindRegionBounds2D(TrainingSet, TrainingSet.Input(worstObjectiveFunctions(i,1),:));
            
            % refine the training set
            parfor j = 1:RefinementPoints
                % pick points at random from a gaussian distribution
                rnd = [0.5.*randn(1) + 0.5, 0.5.*randn(1) + 0.5];
                while rnd(1) < 0.0 || rnd(1) > 1.0 || rnd(2) < 0.0 || rnd(2) > 1.0
                    rnd = [0.5.*randn(1) + 0.5, 0.5.*randn(1) + 0.5];
                end
                SampleInput(j,:) = [RefinedRegionXStart + (RefinedRegionXEnd - RefinedRegionXStart) * rnd(1), RefinedRegionYStart + (RefinedRegionYEnd - RefinedRegionYStart) * rnd(2)];
                SampleOutput(j) = feval(hSimulation, SampleInput(j,:));
            end

            TrainingSet.Input = vertcat(TrainingSet.Input, SampleInput);
            TrainingSet.Output = vertcat(TrainingSet.Output, SampleOutput);
            TrainingSet.Total = TrainingSet.Total + RefinementPoints;
        end      
        worstObjectiveFunction = CT_FindWorstObjectiveFunctions(TrainingSet, 1);  
    end
    
    [TestSet.Input, TestSet.Output, TestSet.Total] = CT_CreateSimulationSamples2D(ValidationSetSize, hSimulation, RegionXStart, RegionXEnd, RegionYStart, RegionYEnd, true);
    
end