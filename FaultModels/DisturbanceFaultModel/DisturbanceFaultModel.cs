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

namespace FM4CC.FaultModels.Disturbance
{
    public class DisturbanceFaultModel : FaultModel
    {
        private bool setupDone;
        private const string prefix = "CT_";
        private const string shortName = "DisturbanceFaultModel";
        private const string name = "Disturbance";
        private const string description = "A disturbance (opposing force) acts upon the output of the process and limits the actual value. The disturbance gets measured by the controller, which tries to compensate for it. At some point, the disturbance is removed, possibly resulting in problems for the controller. \r\nThe disturbance is represented in the model through a from workspace block The input signal can be a trapezoidal ramp, a pulse or a sine wave.";
        private string scriptsPath;

        #region Fault Model Implementation

        public DisturbanceFaultModel(ExecutionEnvironment executionEngine, string scriptsPath)
        {
            setupDone = false;
            this.scriptsPath = scriptsPath;
            this.ExecutionEngine = executionEngine;
            this.FaultModelConfiguration = new DisturbanceFaultModelConfiguration();
            this.RandomExplorationWorker = new RandomExplorationWorker();
            this.WorstCaseWorker = new WorstCaseScenarioWorker();
            this.TestRunWorker = new TestRunWorker();
        }

        public DisturbanceFaultModel(ExecutionEnvironment executionEngine, FaultModelConfiguration configuration, string scriptsPath)
        {
            setupDone = false;
            this.scriptsPath = scriptsPath;
            this.ExecutionEngine = executionEngine;
            this.FaultModelConfiguration = (DisturbanceFaultModelConfiguration)configuration;
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
                ExecutionEngine.PutVariable(prefix + "UserTempPath", System.IO.Path.GetTempPath());
                // Add primitive parameters directly to the execution engine
                Dictionary<string, object>.Enumerator primitiveEnumerator = FaultModelConfiguration.GetParametersEnumerator();

                while (primitiveEnumerator.MoveNext())
                {
                    KeyValuePair<string, object> kvPair = primitiveEnumerator.Current;
                    ExecutionEngine.PutVariable(prefix + kvPair.Key, kvPair.Value);
                }

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

                ExecutionEngine.ChangeWorkingFolder(tempPath);

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
            SetTestRunParameters((double)input["Desired"], (double)input["StartTime"]);
            return ((string)ExecuteSimulationRun()).ToLower().Contains("success");
        }


        public override string Name { get { return name; } }
        public override string Description { get { return description; } }
        public override string ToString()
        {
            return shortName;
        }

#endregion

        #region Disturbance Fault Model
        
        public BackgroundWorker RandomExplorationWorker { get; protected set; }
        public BackgroundWorker WorstCaseWorker { get; protected set; }
        public BackgroundWorker TestRunWorker { get; protected set; }
        
        #endregion

        #region Simulation

        private double desired;
        private double disturbanceStartTime;

        internal void SetTestRunParameters(double desired, double disturbanceStartTime)
        {
            this.desired = desired;
            this.disturbanceStartTime = disturbanceStartTime;
        }

        private object ExecuteSimulationRun()
        {
            if (setupDone)
            {               
                ExecutionEngine.PutVariable(prefix + "DesiredValue", desired);
                ExecutionEngine.PutVariable(prefix + "DisturbanceStartTime", disturbanceStartTime);

                return ExecutionEngine.ExecuteCommand("DisturbanceModelExecution");
            }
            else throw new InvalidOperationException("Setup not performed");
        }

        #endregion

        #region Random Exploration

        private string ExecuteRandomExploration()
        {
            if (setupDone)
            {
                return ExecutionEngine.ExecuteCommand("RandomExploration_Disturbance");
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

            double time = 2 * SimulationSettings.ModelRunningTime * (double)this.FaultModelConfiguration.GetValue("Regions") * (double)this.FaultModelConfiguration.GetValue("Regions") * (double)this.FaultModelConfiguration.GetValue("PointsPerRegion") / (double)coreCount;

            TimeSpan result = new TimeSpan(0, 0, (int)(time));
            return result;
        }


        #endregion

        #region SingleStateSearch

        private object ExecuteWorstCaseSearch()
        {
            if (setupDone)
            {
                return ExecutionEngine.ExecuteCommand("SingleStateSearch_Disturbance");
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
