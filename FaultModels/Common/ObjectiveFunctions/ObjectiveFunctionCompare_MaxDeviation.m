function [MaxDeviation] = ObjectiveFunctionCompare_MaxDeviation(ActualValueSignal_Controller1, ActualValueSignal_Controller2, DesiredValue)
    MaxDeviation = 0;
    indexFinal = length(ActualValueSignal_Controller1);

    for i = 1 : indexFinal
    	if DesiredValue ~= 0
        	Deviation = abs(ActualValueSignal_Controller1(i) - ActualValueSignal_Controller2(i))/(DesiredValue+abs(ActualValueSignal_Controller1(i) - ActualValueSignal_Controller2(i)));
    	else
    		Deviation = 0;
    	end
        if (Deviation > MaxDeviation)
            MaxDeviation = Deviation;
        end
    end
end