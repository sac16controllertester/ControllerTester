function [SampleInput, SampleOutput, SampleTotal] = CT_CreateSimulationSamples2D(SampleCnt, hSimulation, RegionXStart, RegionXEnd, RegionYStart, RegionYEnd, UseRandom)
    % initialize the parallel pool if not created yet
    poolobj = gcp('nocreate');
    if isempty(poolobj)
        poolobj = parpool;
    end
    CT_DiaryLog(strcat('SimulationSamplesStep:',num2str(SampleCnt),',',num2str(poolobj.NumWorkers)));
    
    if UseRandom == 1
        SampleInput = zeros(SampleCnt,2);
        SampleOutput = zeros(SampleCnt,1);
        
        parfor i = 1:SampleCnt
            % pick points at random from the region
            SampleInput(i,:) = [RegionXStart + (RegionXEnd - RegionXStart) * rand(1), RegionYStart + (RegionYEnd - RegionYStart) * rand(1)];
            SampleOutput(i) = feval(hSimulation, SampleInput(i,:));
        end
        
        SampleTotal = SampleCnt;
    else
        % partition the input space equally, convert the sample count to be
        % a perfect square if not already
        PointTotal = (ceil(sqrt(double(SampleCnt))))^2;
        PointsPerAxis = sqrt(PointTotal);
        
        SampleInput = zeros(PointTotal,2);
        SampleOutput = zeros(PointTotal,1);
                
        % calculate the distance between points 
        % (typically the same for both X and Y)
        PointDistanceX = (RegionXEnd - RegionXStart)/(PointsPerAxis-1);
        PointDistanceY = (RegionYEnd - RegionYStart)/(PointsPerAxis-1);
        
        % generate the input
        for i = 1 : PointsPerAxis
            InitialDesiredValue = RegionXStart + (i-1)*PointDistanceX;
            for j = 1 : PointsPerAxis
                DesiredValue = RegionYStart + (j-1)*PointDistanceY;
                SampleInput(j+PointsPerAxis*(i-1),:) = [InitialDesiredValue DesiredValue];
            end
        end
        
        parfor PointCnt = 1 : PointTotal
            % simulate the model with the chosen input
            SampleOutput(PointCnt) = feval(hSimulation, SampleInput(PointCnt,:));
        end
        
        SampleTotal = PointTotal;
    end
end

