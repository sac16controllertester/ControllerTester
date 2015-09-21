function [ RefinedRegionXStart, RefinedRegionXEnd, RefinedRegionYStart, RefinedRegionYEnd ] = CT_FindRegionBounds2D(DataSet, InputPoint)
    lowerBound.Index = 0;
    lowerBound.Value = Inf;

    upperBound.Index = 0;
    upperBound.Value = Inf;

    for i = 1:DataSet.Total
        if (DataSet.Input(i,1)<=InputPoint(1) && DataSet.Input(i,2)<=InputPoint(2))
             dist = sqrt((DataSet.Input(i,1)-InputPoint(1))^2 + (DataSet.Input(i,2)-InputPoint(2))^2);
             if (dist ~= 0 && dist < lowerBound.Value)
                lowerBound.Index = i;
                lowerBound.Value = dist;
             end
        end
        
        if (DataSet.Input(i,1)>=InputPoint(1) && DataSet.Input(i,2)>=InputPoint(2))
             dist = sqrt((DataSet.Input(i,1)-InputPoint(1))^2 + (DataSet.Input(i,2)-InputPoint(2))^2);
             if (dist ~= 0 && dist < upperBound.Value)
                upperBound.Index = i;
                upperBound.Value = dist;
             end
        end        
    end
    
    if lowerBound.Index ~= 0
        RefinedRegionXStart = DataSet.Input(lowerBound.Index,1);
        RefinedRegionYStart = DataSet.Input(lowerBound.Index,2);
    else
        RefinedRegionXStart = InputPoint(1);
        RefinedRegionYStart = InputPoint(2);
    end
    
    if upperBound.Index ~= 0
        RefinedRegionXEnd = DataSet.Input(upperBound.Index,1);
        RefinedRegionYEnd = DataSet.Input(upperBound.Index,2);
    else
        RefinedRegionXEnd = InputPoint(1);
        RefinedRegionYEnd = InputPoint(2);
    end
end

