function output = CT_RegressionPolyAdditive2(param, input)
    a = param(1);
    b = param(2);
    c = param(3);
    d = param(4);
    e = param(5);
    
    % poly2
    N = size(input,1);
    output = zeros(N,1);
    for i = 1:N
        output(i) = a + b*input(i,1) + c*input(i,2) + d*input(i,2)^2 + e*input(i,1)^2;
    end
end