using FM4CC.ExecutionEngine;
using FM4CC.FaultModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FM4CC.FaultModels.Disturbance
{
    internal class TestRunWorker: BackgroundWorker
    {
        internal FM4CCException Exception { get; set; }

        internal double Desired { get; set; }
        internal double DisturbanceStartTime { get; set; }

        internal TestRunWorker()
        {
            this.Desired = 0;
            this.DisturbanceStartTime = 0;
            this.WorkerReportsProgress = true;
            this.DoWork += testRunWorker_DoWork;
        }

        private void testRunWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                this.Exception = null;
                DisturbanceFaultModel fm = e.Argument as DisturbanceFaultModel;

                fm.ExecutionEngine.AcquireProcess();

                // Sets up the environment of the execution engine
                fm.SetUpEnvironment();

                fm.SetTestRunParameters(Desired, DisturbanceStartTime);
                string message = (string)fm.Run("TestRun");

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
            }
            catch (TargetInvocationException)
            {
                e.Result = false;
            }
        }
    }
}
