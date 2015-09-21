function [MeanDeviation] = ObjectiveFunctionCompare_MeanDeviation(ActualValueSignal_Controller1, ActualValueSignal_Controller2, DesiredValue)
    MeanDeviation = 0;
    lengthSignal = length(ActualValueSignal_Controller1);

    for i = 1 : lengthSignal
    	if DesiredValue ~= 0
        	Deviation = abs(ActualValueSignal_Controller1(i) - ActualValueSignal_Controller2(i))/(DesiredValue+abs(ActualValueSignal_Controller1(i) - ActualValueSignal_Controller2(i)));
        else
        	Deviation = 0;
        end
        MeanDeviation = MeanDeviation + Deviation;
    end

    MeanDeviation = MeanDeviation / lengthSignal;

end