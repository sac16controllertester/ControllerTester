using FM4CC.ExecutionEngine;
using FM4CC.FaultModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FM4CC.FaultModels.ControllerComparison
{
    internal class TestRunWorker: BackgroundWorker
    {
        internal FM4CCException Exception;
        internal enum TestModel { ProjectModel, ComparedModel, Both };

        internal TestModel ExecuteOnModel { get; set; }
        internal double InitialDesired { get; set; }
        internal double FinalDesired { get; set; }


        public TestRunWorker()
        {
            this.Exception = null;
            this.ExecuteOnModel = TestModel.ProjectModel;
            this.InitialDesired = 0;
            this.FinalDesired = 0;
            this.WorkerReportsProgress = true;
            this.DoWork += testRunWorker_DoWork;
        }

        private void testRunWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                this.Exception = null;

                string message = "";
                ControllerComparisonFaultModel fm = e.Argument as ControllerComparisonFaultModel;

                fm.ExecutionEngine.AcquireProcess();
                
                // Sets up the environment of the execution engine
                fm.SetUpEnvironment();

                fm.SetTestRunParameters(InitialDesired, FinalDesired);
            
                switch(ExecuteOnModel)
                {
                    case TestModel.ProjectModel:
                        message = (string)fm.Run("TestRun");
                        break;
                    case TestModel.ComparedModel:
                        message = (string)fm.Run("TestRun_Compared");
                        break;
                    case TestModel.Both:
                        message = (string)fm.Run("CompareRun");
                        break;
                }

                // Tears down the environment
                fm.TearDownEnvironment(false);

                // Relinquishes control of the execution engine
                fm.ExecutionEngine.RelinquishProcess();

                if (message.ToLower().Contains("success"))
                {
                    e.Result = true;
                }
                else
                {
                    e.Result = false;
                    this.Exception = new FM4CCException(message);
                }

                this.ReportProgress(100);
                e.Result = true;
            }
            catch(TargetInvocationException)
            {
                e.Result = false;
            }            
        }
    }
}