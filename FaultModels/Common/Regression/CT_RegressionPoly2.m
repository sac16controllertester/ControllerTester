function output = CT_RegressionPoly2(b, input)
    % poly2
    N = size(input,1);
    output = zeros(N,1);
    for i = 1:N
        output(i) = b(1) + b(2)*input(i,1) + b(3)*input(i,2) + b(4)*input(i,2)*input(i,1) + b(5)*input(i,1)^2 + b(6)*input(i,2)^2;
    end
end