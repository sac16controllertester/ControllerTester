function [mdl, error, regressionFunction, bestRMSE, bestFunction, modelQualityThreshold] = CT_ChooseBestRegression(trainingSet, testSet, modelQuality, worstObjectiveFunctionIndex)
    modelQualityThreshold = (modelQuality/100)*trainingSet.Output(worstObjectiveFunctionIndex)*-1;
    options=optimset('display','off');

    % try Linear Regression
    mdlOptions(1).Parameters = 3;
    [mdlOptions(1).Regression, resnorm, residuals] = lsqcurvefit(@CT_RegressionLinear,ones(mdlOptions(1).Parameters,1),trainingSet.Input,trainingSet.Output, [], [], options);
    mdlOptions(1).RMSE = std(residuals);
    mdlOptions(1).Function = @CT_RegressionLinear;
    
    % try Exponential Regression
    mdlOptions(2).Parameters = 2;
    [mdlOptions(2).Regression, resnorm, residuals] = lsqcurvefit(@CT_RegressionExp,ones(mdlOptions(2).Parameters,1),trainingSet.Input,trainingSet.Output, [], [], options);
    mdlOptions(2).RMSE = std(residuals);
    mdlOptions(2).Function = @CT_RegressionExp;

    % try Additive Polynomial Regression of order 2
    mdlOptions(3).Parameters = 5;
    [mdlOptions(3).Regression, resnorm, residuals] = lsqcurvefit(@CT_RegressionPolyAdditive2,ones(mdlOptions(3).Parameters,1),trainingSet.Input,trainingSet.Output, [], [], options);
    mdlOptions(3).RMSE = std(residuals);
    mdlOptions(3).Function = @CT_RegressionPolyAdditive2;
    
    % try Additive Polynomial Regression of order 3
    mdlOptions(4).Parameters = 7;
    [mdlOptions(4).Regression, resnorm, residuals] = lsqcurvefit(@CT_RegressionPolyAdditive3,ones(mdlOptions(4).Parameters,1),trainingSet.Input,trainingSet.Output, [], [], options);
    mdlOptions(4).RMSE = std(residuals);
    mdlOptions(4).Function = @CT_RegressionPolyAdditive3;

    % try Additive Polynomial Regression of order 4
    mdlOptions(5).Parameters = 9;
    [mdlOptions(5).Regression, resnorm, residuals] = lsqcurvefit(@CT_RegressionPolyAdditive4,ones(mdlOptions(5).Parameters,1),trainingSet.Input,trainingSet.Output, [], [], options);
    mdlOptions(5).RMSE = std(residuals);
    mdlOptions(5).Function = @CT_RegressionPolyAdditive4;

    % try Non-additive Polynomial Regression 2
    mdlOptions(6).Parameters = 6;
    [mdlOptions(6).Regression, resnorm, residuals] = lsqcurvefit(@CT_RegressionPoly2,ones(mdlOptions(6).Parameters,1),trainingSet.Input,trainingSet.Output, [], [], options);
    mdlOptions(6).RMSE = std(residuals);
    mdlOptions(6).Function = @CT_RegressionPoly2;
    
    % try Non-additive Polynomial Regression 3
    mdlOptions(7).Parameters = 10;
    [mdlOptions(7).Regression, resnorm, residuals] = lsqcurvefit(@CT_RegressionPoly3,ones(mdlOptions(7).Parameters,1),trainingSet.Input,trainingSet.Output, [], [], options);
    mdlOptions(7).RMSE = std(residuals);
    mdlOptions(7).Function = @CT_RegressionPoly3;

    % try Non-additive Polynomial Regression 4
    mdlOptions(8).Parameters = 15;
    [mdlOptions(8).Regression, resnorm, residuals] = lsqcurvefit(@CT_RegressionPoly4,ones(mdlOptions(8).Parameters,1),trainingSet.Input,trainingSet.Output, [], [], options);
    mdlOptions(8).RMSE = std(residuals);
    mdlOptions(8).Function = @CT_RegressionPoly4;

    % try Non-additive Polynomial Regression 8
    mdlOptions(9).Parameters = 45;
    [mdlOptions(9).Regression, resnorm, residuals] = lsqcurvefit(@CT_RegressionPoly8,ones(mdlOptions(9).Parameters,1),trainingSet.Input,trainingSet.Output,[],[], options);
    mdlOptions(9).RMSE = std(residuals); 
    mdlOptions(9).Function = @CT_RegressionPoly8;

    % find the first model which fits the data    
    mdl.Parameters=0;
    % track the best/chosen RMSE and function
    bestRMSE = Inf;
    bestFunctionIndex = 0;
    regressionFunction = 'None';
    
    for i = 1:9
        % store the best/chosen RMSE
        if (mdlOptions(i).RMSE < bestRMSE)
            bestRMSE = mdlOptions(i).RMSE;
            bestFunctionIndex = i;
        end
        if (mdlOptions(i).RMSE <= modelQualityThreshold)
            % check for fitting errors in the residuals of the training set
            % relatve to the quality threshold
            for j = 1 : trainingSet.Total
                if (abs(residuals(j)) > modelQualityThreshold)
                    % high fitting errors present, try next model
                    continue;
                end
            end
            
            % check for fitting errors in the residuals of the test set
            % relatve to the quality threshold
            testSetPredictions = mdlOptions(i).Function(mdlOptions(i).Regression, testSet.Input);
            for j = 1 : testSet.Total
                if (abs(testSetPredictions(j)-testSet.Output(j)) > modelQualityThreshold)
                    % high fitting errors present, try next model
                    continue;
                end            
            end
            
            % model fits well to data, choose it
            mdl = mdlOptions(i);
            break;
        end
    end
    
    % convert to string for logging
    switch(i)
        case 1
            regressionFunction = 'Linear';
        case 2
            regressionFunction = 'Exponential';
        case 3
            regressionFunction = 'PolyAdditive2';
        case 4
            regressionFunction = 'PolyAdditive3';
        case 5
            regressionFunction = 'PolyAdditive4';
        case 6
            regressionFunction = 'Poly2';
        case 7
            regressionFunction = 'Poly3';
        case 8
            regressionFunction = 'Poly4';
        case 9
            regressionFunction = 'Poly8';
    end
    
    % convert to string for logging
    switch(bestFunctionIndex)
        case 1
            bestFunction = 'Linear';
        case 2
            bestFunction = 'Exponential';
        case 3
            bestFunction = 'PolyAdditive2';
        case 4
            bestFunction = 'PolyAdditive3';
        case 5
            bestFunction = 'PolyAdditive4';
        case 6
            bestFunction = 'Poly2';
        case 7
            bestFunction = 'Poly3';
        case 8
            bestFunction = 'Poly4';
        case 9
            bestFunction = 'Poly8';
    end
    
    if (mdl.Parameters == 0)
        error = true;
    else
        error = false;
    end
end