function output = CT_RegressionLinear(param, input)
    a = param(1);
    b = param(2);
    c = param(3);
    % linear
    N = size(input,1);
    output = zeros(N,1);
    for i = 1:N
        output(i) = a + b*input(i,1) + c*input(i,2);
    end
end