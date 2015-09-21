function output = CT_RegressionPoly8(b, input)
    % poly8
    N = size(input,1);
    output = zeros(N,1);
    for i = 1:N
        Var1 = input(i,1);
        Var2 = input(i,2);
        output(i) = b(1) + b(2)*Var1 + b(3)*Var2 + b(4)*Var1*Var2 + b(5)*Var1^2 + b(6)*Var2^2 + b(7)*Var1^2*Var2 + b(8)*Var1*Var2^2 + b(9)*Var1^3 + b(10)*Var2^3 + b(11)*Var1^2*Var2^2 + b(12)*Var1^3*Var2 + b(13)*Var1*Var2^3 + b(14)*Var1^4 + b(15)*Var2^4 + ...
    		b(16)*Var1^3*Var2^2 + b(17)*Var1^2*Var2^3 + b(18)*Var1^4*Var2 + b(19)*Var1*Var2^4 + b(20)*Var1^5 + b(21)*Var2^5 + b(22)*Var1^3*Var2^3 + b(23)*Var1^4*Var2^2 + b(24)*Var1^2*Var2^4 + b(25)*Var1^5*Var2 + b(26)*Var1*Var2^5 + ...
    		b(27)*Var1^6 + b(28)*Var2^6 + b(29)*Var1^4*Var2^3 + b(30)*Var1^3*Var2^4 + b(31)*Var1^5*Var2^2 + b(32)*Var1^2*Var2^5 + b(33)*Var1^6*Var2 + b(34)*Var1*Var2^6 + b(35)*Var1^7 + b(36)*Var2^7 + b(37)*Var1^4*Var2^4 + b(38)*Var1^5*Var2^3 + ...
    		b(39)*Var1^3*Var2^5+b(40)*Var1^6*Var2^2+b(41)*Var1^2*Var2^6 +b(42)*Var1^7*Var2 + b(43)*Var1*Var2^7 + b(44)*Var1^8 + b(45)*Var2^8;
    end
end