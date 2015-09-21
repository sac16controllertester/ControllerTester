function output = CT_RegressionPoly4(b, input)
    % poly4
    N = size(input,1);
    output = zeros(N,1);
    for i = 1:N
        Var1 = input(i,1);
        Var2 = input(i,2);
        output(i) = b(1) + b(2)*Var1 + b(3)*Var2 + b(4)*Var1*Var2 + b(5)*Var1^2 + b(6)*Var2^2 + b(7)*Var1^2*Var2 + b(8)*Var1*Var2^2 + b(9)*Var1^3 + b(10)*Var2^3 + b(11)*Var1^2*Var2^2 + b(12)*Var1^3*Var2 + b(13)*Var1*Var2^3 + b(14)*Var1^4 + b(15)*Var2^4;
    end
end