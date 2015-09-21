using FM4CC.FaultModels;
using FM4CC.ExecutionEngine;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using FM4CC.Environment;
using System.ComponentModel;
using FM4CC.Util.Heatmap;
using System.Globalization;
using FM4CC.TestCase;

namespace FM4CC.FaultModels.ControllerComparison
{
    public class ControllerComparisonFaultModel : FaultModel
    {
        private bool setupDone;
        private const string prefix = "CT_";
        private const string shortName = "ControllerComparisonFaultModel";
        private const string name = "Controller Comparison";
        private const string description = "The output of a controller model is compared to a similar controller model by simulating both in parallel, using the Step Fault Model as a basis, and computing the maximum and the mean difference besides the standard objective functions of the Step Fault Model. A typical use case for this fault model is when we have an original, continuous, model and a derived discrete model, from which the code is generated. We want to verify that the discretized model's output is approximately equal to the original model and check for any major discrepancies, indicating a problem in the discretized model.\r\n\r\nInput and output sanitization and conversion is assumed to be present in the derived model, such that both models can be run with the same input and the length of the output is the same in the end. If not present, please introduce this into the model, else the two cannot be compared. \r\n\r\nThe models must have different names, else the comparison will fail.";
        private string scriptsPath;

        #region Fault Model Implementation

        public ControllerComparisonFaultModel(ExecutionEnvironment executionEngine, string scriptsPath)
        {
            setupDone = false;
            this.scriptsPath = scriptsPath;
            this.ExecutionEngine = executionEngine;
            this.FaultModelConfiguration = new ControllerComparisonFaultModelConfiguration();
            this.RandomExplorationWorker = new RandomExplorationWorker();
            this.WorstCaseWorker = new WorstCaseScenarioWorker();
            this.TestRunWorker = new TestRunWorker();
        }

        public ControllerComparisonFaultModel(ExecutionEnvironment executionEngine, FaultModelConfiguration configuration, string scriptsPath)
        {
            setupDone = false;
            this.scriptsPath = scriptsPath;
            this.ExecutionEngine = executionEngine;
            this.FaultModelConfiguration = (ControllerComparisonFaultModelConfiguration)configuration;
            this.RandomExplorationWorker = new RandomExplorationWorker();
            this.WorstCaseWorker = new WorstCaseScenarioWorker();
            this.TestRunWorker = new TestRunWorker();
        }

        public override void SetUpEnvironment()
        {
            if (!setupDone)
            {
                // Get hold of the execution engine if not already
                ExecutionEngine.AcquireProcess();

                // Clear the workspace
                ExecutionEngine.ExecuteCommand("clear all;");
                ExecutionEngine.ExecuteCommand("pctRunOnAll clear all;");

                ExecutionEngine.PutVariable(prefix + "AccelerationDisabled", false);
                ExecutionEngine.PutVariable(prefix + "ScriptsPath", scriptsPath);

                string modelFile = ExecutionInstance.GetValue("SUTPath");
                string modelPath = modelFile.Substring(0, modelFile.LastIndexOf('\\'));

                ExecutionEngine.PutVariable(prefix + "ModelFile", modelFile);
                ExecutionEngine.PutVariable(prefix + "ModelPath", modelPath);
                                
                ExecutionEngine.PutVariable(prefix + "ModelConfigurationFile", ExecutionInstance.GetValue("SUTSettingsPath"));
                ExecutionEngine.PutVariable(prefix + "ModelSimulationTime", SimulationSettings.ModelSimulationTime);

                string tempPath = modelPath + "\\ControllerTesterResults";
                if (!Directory.Exists(tempPath))
                {
                    Directory.CreateDirectory(tempPath);
                }
                ExecutionEngine.PutVariable(prefix + "TempPath", tempPath);

                // change the working folder to the original model's folder
                ExecutionEngine.ChangeWorkingFolder(tempPath);

                ExecutionEngine.PutVariable(prefix + "UserTempPath", System.IO.Path.GetTempPath());

                // Add primitive parameters directly to the execution engine 
                // and adds the compared model as well as its DesiredVariableName and ActualVariableName
                Dictionary<string, object>.Enumerator primitiveEnumerator = FaultModelConfiguration.GetParametersEnumerator();

                while (primitiveEnumerator.MoveNext())
                {
                    KeyValuePair<string, object> kvPair = primitiveEnumerator.Current;
                    ExecutionEngine.PutVariable(prefix + kvPair.Key, kvPair.Value);
                }

                // Add the compared model's path
                string comparedModelFile = (string)FaultModelConfiguration.GetValue("Compared_ModelFile");
                string comparedModelPath = comparedModelFile.Substring(0, comparedModelFile.LastIndexOf('\\'));

                ExecutionEngine.PutVariable(prefix + "Compared_ModelPath", comparedModelPath);

                tempPath = comparedModelPath + "\\ControllerTesterResults";
                if (!Directory.Exists(tempPath))
                {
                    Directory.CreateDirectory(tempPath);
                }
                ExecutionEngine.PutVariable(prefix + "Compared_TempPath", tempPath);
                
                ExecutionEngine.PutVariable(prefix + "DesiredValueRangeStart", Convert.ToDouble(this.SimulationSettings.DesiredVariable.FromValue));
                ExecutionEngine.PutVariable(prefix + "DesiredValueRangeEnd", Convert.ToDouble(this.SimulationSettings.DesiredVariable.ToValue));
                ExecutionEngine.PutVariable(prefix + "DesiredVariableName", this.SimulationSettings.DesiredVariable.Name);

                ExecutionEngine.PutVariable(prefix + "ActualValueRangeStart", Convert.ToDouble(this.SimulationSettings.ActualVariable.FromValue));
                ExecutionEngine.PutVariable(prefix + "ActualValueRangeEnd", Convert.ToDouble(this.SimulationSettings.ActualVariable.ToValue));
                ExecutionEngine.PutVariable(prefix + "ActualVariableName", this.SimulationSettings.ActualVariable.Name);

                ExecutionEngine.PutVariable(prefix + "DisturbanceAmplitude", Convert.ToDouble(this.SimulationSettings.DisturbanceVariable.ToValue));
                ExecutionEngine.PutVariable(prefix + "DisturbanceVariableName", this.SimulationSettings.DisturbanceVariable.Name);

                ExecutionEngine.PutVariable(prefix + "TimeStable", this.SimulationSettings.StableStartTime);
                ExecutionEngine.PutVariable(prefix + "SmoothnessStartDifference", this.SimulationSettings.SmoothnessStartDifference);
                ExecutionEngine.PutVariable(prefix + "ResponsivenessClose", this.SimulationSettings.ResponsivenessClose);

                // add scripts to path
                ExecutionEngine.ExecuteCommand("addpath(strcat(CT_ScriptsPath, '\\ModelExecution'));");
                ExecutionEngine.ExecuteCommand("addpath(strcat(CT_ScriptsPath, '\\RandomExploration'));");
                ExecutionEngine.ExecuteCommand("addpath(strcat(CT_ScriptsPath, '\\SingleStateSearch'));");

                setupDone = true;
            }
            else throw new FM4CCException("An execution instance is already under way. Please tear down the environment first!");
        }
    
        public override void TearDownEnvironment(bool relinquishExectionEngineControl = true)
        {
            // Clear the workspace
//            ExecutionEngine.ExecuteCommand("clear all;");
            if (relinquishExectionEngineControl)
            {
                ExecutionEngine.RelinquishProcess();
            }
            setupDone = false;
        }

        public override IList<string> GetSteps()
        {
            return new List<string>() { "RandomExploration", "WorstCaseSearch" };
        }

        public override TimeSpan GetEstimatedDuration(string step)
        {
            switch(step)
            {
                case "RandomExploration": 
                    return GetRandomExplorationEstimatedRunningTime();
                case "WorstCaseSearch":
                    return GetSingleStateEstimatedRunningTime();
                default:
                    return new TimeSpan(1000);
            }

        }

        public override object Run(string step, params object[] args)
        {
            switch(step)
            {
                case "TestRun":
                    return ExecuteSimulationRun();
                case "TestRun_Compared":
                    return ExecuteSimulationRunCompared();
                case "CompareRun":
                    return ExecuteComparisonRun();
                case "RandomExploration": 
                    return ExecuteRandomExploration();
                case "WorstCaseSearch":
                    return ExecuteWorstCaseSearch();
                default:
                    throw new FM4CCException("No such step exists");
            }
        }

        public override bool Run(FaultModelTesterTestCase testCase)
        {
            IDictionary<string, object> input = testCase.Input;
            SetTestRunParameters((double)input["Initial"], (double)input["Final"]);
            return ((string)ExecuteComparisonRun()).ToLower().Contains("success");
        }


        public override string Name { get { return name; } }
        public override string Description { get { return description; } }
        public override string ToString()
        {
            return shortName;
        }

#endregion

        #region Controller Comparison Fault Model
        
        public BackgroundWorker RandomExplorationWorker { get; protected set; }
        public BackgroundWorker WorstCaseWorker { get; protected set; }
        public BackgroundWorker TestRunWorker { get; protected set; }
        
        #endregion

        #region Simulation

        private double initial;
        private double final;

        internal void SetTestRunParameters(double initial, double final)
        {
            this.initial = initial;
            this.final = final;
        }

        private object ExecuteSimulationRun()
        {
            if (setupDone)
            {                
                ExecutionEngine.PutVariable(prefix + "InitialDesiredValue", initial);
                ExecutionEngine.PutVariable(prefix + "DesiredValue", final);

                return ExecutionEngine.ExecuteCommand("StepModelExecution");
            }
            else throw new InvalidOperationException("Setup not performed");
        }

        private object ExecuteComparisonRun()
        {
            if (setupDone)
            {
                ExecutionEngine.PutVariable(prefix + "InitialDesiredValue", initial);
                ExecutionEngine.PutVariable(prefix + "DesiredValue", final);
                
                return ExecutionEngine.ExecuteCommand("StepModelComparisonExecution");
            }
            else throw new InvalidOperationException("Setup not performed");
        }

        private object ExecuteSimulationRunCompared()
        {
            if (setupDone)
            {
                // Overrides the chosen model with the compared model
                string comparedModelFile = (string)FaultModelConfiguration.GetValue("Compared_ModelFile");
                ExecutionEngine.PutVariable(prefix + "ModelFile", comparedModelFile);
                string comparedModelPath = comparedModelFile.Substring(0, comparedModelFile.LastIndexOf('\\'));

                ExecutionEngine.PutVariable(prefix + "ModelPath", comparedModelPath);

                string tempPath = comparedModelPath + "\\ControllerTesterResults";
                if (!Directory.Exists(tempPath))
                {
                    Directory.CreateDirectory(tempPath);
                }
                ExecutionEngine.PutVariable(prefix + "TempPath", tempPath);

                ExecutionEngine.PutVariable(prefix + "ModelConfigurationFile", FaultModelConfiguration.GetValue("Compared_ModelConfigurationFile"));

                ExecutionEngine.PutVariable(prefix + "ActualVariableName", (string)FaultModelConfiguration.GetValue("Compared_ActualVariableName"));
                ExecutionEngine.PutVariable(prefix + "DesiredVariableName", (string)FaultModelConfiguration.GetValue("Compared_DesiredVariableName"));
                
                ExecutionEngine.PutVariable(prefix + "InitialDesiredValue", initial);
                ExecutionEngine.PutVariable(prefix + "DesiredValue", final);

                return ExecutionEngine.ExecuteCommand("StepModelExecution");
            }
            else throw new InvalidOperationException("Setup not performed");
        }

        #endregion

        #region Random Exploration

        private string ExecuteRandomExploration()
        {
            if (setupDone)
            {
                return ExecutionEngine.ExecuteCommand("RandomExplorationComparison_Step");
            }
            else throw new InvalidOperationException("Setup not performed");
        }

        private TimeSpan GetRandomExplorationEstimatedRunningTime()
        {
            // get number of cores, since MATLAB's Parallel Computing Toolbox typically starts the same amount of workers
            int coreCount = 0;
            foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_Processor").Get())
            {
                coreCount += int.Parse(item["NumberOfCores"].ToString());
            }

            double time = 2 * SimulationSettings.ModelRunningTime * 2 * (double)this.FaultModelConfiguration.GetValue("Regions") * (double)this.FaultModelConfiguration.GetValue("Regions") * (double)this.FaultModelConfiguration.GetValue("PointsPerRegion") / (double)coreCount;

            TimeSpan result = new TimeSpan(0, 0, (int)(time));
            return result;
        }


        #endregion

        #region SingleStateSearch

        private object ExecuteWorstCaseSearch()
        {
            if (setupDone)
            {
                return ExecutionEngine.ExecuteCommand("SingleStateSearchComparison_Step");
            }
            else throw new InvalidOperationException("Setup not performed");
        }

        public void SetSearchParameters(int requirement, int regionX, int regionY, HeatPoint startPoint, string optimizationAlgorithm)
        {
            if (setupDone)
            {
                ExecutionEngine.PutVariable(prefix + "MaxObjectiveFunctionIndex", requirement);
                ExecutionEngine.PutVariable(prefix + "RegionXIndex", (double)regionX);
                ExecutionEngine.PutVariable(prefix + "RegionYIndex", (double)regionY);

                ExecutionEngine.PutVariable(prefix + "StartPoint", new double[] { startPoint.X, startPoint.Y });

                ExecutionEngine.PutVariable(prefix + "ModelQuality", SimulationSettings.RegressionSettings.ModelQuality);

                ExecutionEngine.PutVariable(prefix + "RefinedCandidatePoints", SimulationSettings.RegressionSettings.RefinedCandidatePoints);
                ExecutionEngine.PutVariable(prefix + "RefinementPoints", SimulationSettings.RegressionSettings.RefinementPoints);

                ExecutionEngine.PutVariable(prefix + "TrainingSetSizeEqualDistance", SimulationSettings.RegressionSettings.TrainingSetSizeEqualDistance);
                ExecutionEngine.PutVariable(prefix + "TrainingSetSizeRandom", SimulationSettings.RegressionSettings.TrainingSetSizeRandom);
                ExecutionEngine.PutVariable(prefix + "ValidationSetSize", SimulationSettings.RegressionSettings.ValidationSetSize);
                ExecutionEngine.PutVariable(prefix + "OptimizationAlgorithm", optimizationAlgorithm);
            }
        }
     
        private TimeSpan GetSingleStateEstimatedRunningTime()
        {
            double num = 0.0;
            List<string> requirements = this.FaultModelConfiguration.GetValue("Requirements", "complex") as List<string>;
            // TODO
            num = 0;
            
            TimeSpan result = new TimeSpan(0, 0, (int)(num));
            return result;
        }

        #endregion

    }
}
