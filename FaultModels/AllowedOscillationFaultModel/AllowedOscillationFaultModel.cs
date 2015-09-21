using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;
using System.Globalization;

using FM4CC.FaultModels;
using FM4CC.ExecutionEngine;
using FM4CC.Environment;
using FM4CC.Util.Heatmap;
using FM4CC.TestCase;

namespace FM4CC.FaultModels.AllowedOscillation
{
    public class AllowedOscillationFaultModel : FaultModel
    {
        private bool setupDone;
        private const string prefix = "CT_";
        private const string shortName = "AllowedOscillationFaultModel";
        private const string name = "Allowed Oscillation";
        private const string description = "A common requirement for a controller is that when the difference between actual and desired value is less than a preset value the controller should take no action. This usually appears when the controlled system is known to have small oscillations upon reaching and stabilizing at the desired value. Given a controller-plant model and a desired value Desired, the algorithm computes four test cases, two below the limits of the allowed oscillation boundaries and two above the limits. To simulate starting from any initial arbitrary value, the function Desired(t) is defined in a similar way to the step fault model.";
        private string scriptsPath;

        #region Fault Model Implementation

        public AllowedOscillationFaultModel(ExecutionEnvironment executionEngine, string scriptsPath)
        {
            setupDone = false;
            this.scriptsPath = scriptsPath;
            this.ExecutionEngine = executionEngine;
            this.FaultModelConfiguration = new AllowedOscillationFaultModelConfiguration();
            this.SearchSpaceExplorationWorker = new SearchSpaceExplorationWorker();
            this.TestRunWorker = new TestRunWorker();
        }

        public AllowedOscillationFaultModel(ExecutionEnvironment executionEngine, FaultModelConfiguration configuration, string scriptsPath)
        {
            setupDone = false;
            this.scriptsPath = scriptsPath;
            this.ExecutionEngine = executionEngine;
            this.FaultModelConfiguration = (AllowedOscillationFaultModelConfiguration)configuration;
            this.SearchSpaceExplorationWorker = new SearchSpaceExplorationWorker();
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

                ExecutionEngine.PutVariable(prefix + "AllowedOscillationPercentage", (double)this.FaultModelConfiguration.GetValue("AllowedOscillationPercentage"));
                ExecutionEngine.PutVariable(prefix + "DesiredValueStepSize", (double)this.FaultModelConfiguration.GetValue("DesiredValueStepSize"));
                ExecutionEngine.PutVariable(prefix + "TimeStable", this.SimulationSettings.StableStartTime);
                ExecutionEngine.PutVariable(prefix + "GenerateSineWaveTestCases", FaultModelConfiguration.GetValue("GenerateSineWaveTestCases"));

                // add scripts to path
                ExecutionEngine.ExecuteCommand("addpath(strcat(CT_ScriptsPath, '\\ModelExecution'));");
                ExecutionEngine.ExecuteCommand("addpath(strcat(CT_ScriptsPath, '\\AllowedOscillation'));");

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
            return new List<string>() { "SearchSpaceExploration" };
        }

        public override TimeSpan GetEstimatedDuration(string step)
        {
            switch(step)
            {
                case "SearchSpaceExploration": 
                    return GetSearchSpaceExplorationEstimatedRunningTime();
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
                case "SearchSpaceExploration": 
                    return ExecuteSearchSpaceExploration();
                default:
                    throw new FM4CCException("No such step exists");
            }
        }

        public override bool Run(FaultModelTesterTestCase testCase)
        {
            IDictionary<string, object> input = testCase.Input;
            SetTestRunParameters((double)input["Desired"], (int)input["TestCaseType"]);
            return (bool)(((List<object>)ExecuteSimulationRun())[1]);
        }


        public override string Name { get { return name; } }
        public override string Description { get { return description; } }
        public override string ToString()
        {
            return shortName;
        }

        #endregion

        #region Allowed Oscillation Fault Model
        
        public BackgroundWorker SearchSpaceExplorationWorker { get; protected set; }
        public BackgroundWorker TestRunWorker { get; protected set; }
        
        #endregion

        #region Simulation

        private double desired;
        private int testCase;

        /// <summary>
        /// Sets a single desired value for which four test cases will be generated, attempting to void requirements
        /// </summary>
        /// <param name="desired">The desired value for which four test cases will be computed</param>
        internal void SetTestRunParameters(double desired, int testCase = 1)
        {
            this.desired = desired;
            this.testCase = testCase;
        }
        
        private object ExecuteSimulationRun()
        {
            if (setupDone)
            {                
                ExecutionEngine.PutVariable(prefix + "DesiredValue", desired);
                ExecutionEngine.PutVariable(prefix + "SelectedTestCase", testCase);

                string message = ExecutionEngine.ExecuteCommand("TestRun_AllowedOscillation");
                
                string searchPattern = "passed=(0|1)";
                Regex regex = new Regex(searchPattern);
                Match match = regex.Match(message);
                bool result = false;

                if (match.Groups.Count == 2)
                {
                    // the system is run using a step desired value signal
                    // this results in the simulation time being double the normally set simulation time
                    // therefore the default duration is half
                    int val = Int32.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) ;
                    result = Convert.ToBoolean(val);
                }

                return new List<object>() {message, result};
            }
            else throw new InvalidOperationException("Setup not performed");
        }

        private object ExecuteSearchSpaceExploration()
        {
            if (setupDone)
            {
                string message = ExecutionEngine.ExecuteCommand("SearchSpaceExploration_AllowedOscillation");

                return message;
            }
            else throw new InvalidOperationException("Setup not performed");
        }

        #endregion

         private TimeSpan GetSearchSpaceExplorationEstimatedRunningTime()
        {
            // get number of cores, since MATLAB's Parallel Computing Toolbox typically starts the same amount of workers
            int coreCount = 0;
            foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_Processor").Get())
            {
                coreCount += int.Parse(item["NumberOfCores"].ToString());
            }

            int testCases = (bool)FaultModelConfiguration.GetValue("GenerateSineWaveTestCases") == true ? 6 : 4;
            double num = 2 * SimulationSettings.ModelRunningTime * (Convert.ToDouble(this.SimulationSettings.DesiredVariable.ToValue - this.SimulationSettings.DesiredVariable.FromValue) / (double)this.FaultModelConfiguration.GetValue("DesiredValueStepSize")) * (testCases / (double)coreCount);
            TimeSpan result = new TimeSpan(0, 0, (int)(num));
            return result;
        }
    }
}
