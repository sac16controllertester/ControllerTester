function output = SingleStateSearch_PolynomialThird(b, input)
    % exp
    N = size(input,1);
    output = zeros(N,1);
    for i = 1:N
        output(i) = b(1) + b(2)*input(i,1)*input(i,2);
    end
end