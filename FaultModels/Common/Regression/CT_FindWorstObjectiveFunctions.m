function [maxObjectiveFunctionsIndex] = CT_FindWorstObjectiveFunctions(Set, Points)                
    maxObjectiveFunctionsIndex = zeros(Points,1);
    maxObjectiveFunctionsValue = ones(Points,1)*-1;
    
    % input is a set prepared for minimization (since MATLAB's Optimization Toolbox only
    % supports minimization by default)
    for i = 1 : Set.Total
        if (-1*Set.Output(i) >= maxObjectiveFunctionsValue(1,1))
            for j = Points-1 : -1 : 1
                maxObjectiveFunctionsValue(j+1,1) = maxObjectiveFunctionsValue(j,1);
                maxObjectiveFunctionsIndex(j+1,1) = maxObjectiveFunctionsIndex(j,1);
            end
            maxObjectiveFunctionsValue(1,1) = -1*Set.Output(i);
            maxObjectiveFunctionsIndex(1,1) = i;
        end
    end
end