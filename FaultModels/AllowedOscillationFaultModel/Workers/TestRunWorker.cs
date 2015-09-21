using FM4CC.ExecutionEngine;
using FM4CC.FaultModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FM4CC.FaultModels.AllowedOscillation
{
    internal class TestRunWorker: BackgroundWorker
    {
        internal FM4CCException Exception { get; set; }

        internal double Desired { get; set; }
        internal int TestCase { get; set; }

        internal TestRunWorker()
        {
            this.Exception = null;

            this.Desired = 0;
            this.TestCase = 1;
            this.WorkerReportsProgress = true;
            this.DoWork += testRunWorker_DoWork;
        }

        private void testRunWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                this.Exception = null;
                AllowedOscillationFaultModel fm = e.Argument as AllowedOscillationFaultModel;

                fm.ExecutionEngine.AcquireProcess();

                // Sets up the environment of the execution engine
                fm.SetUpEnvironment();

                fm.SetTestRunParameters(Desired, TestCase);
                List<object> results = (List<object>)fm.Run("TestRun");

                string message = (string)results[0];
                bool result = (bool)results[1];

                // Tears down the environment
                fm.TearDownEnvironment(false);

                // Relinquishes control of the execution engine
                fm.ExecutionEngine.RelinquishProcess();
                
                if (result)
                {
                    e.Result = true;
                }
                else
                {
                    e.Result = false;
                    if (!message.ToLower().Contains("success"))
                    {
                        this.Exception = new FM4CCException(message);
                    }
                }

                this.ReportProgress(100);
            }
            catch(TargetInvocationException)
            {
                e.Result = false;
            }
        }
    }
}
