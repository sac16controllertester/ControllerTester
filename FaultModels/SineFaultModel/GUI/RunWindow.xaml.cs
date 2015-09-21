using FM4CC.Util;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FM4CC.FaultModels.Sine.GUI
{
    /// <summary>
    /// Interaction logic for RunWindow.xaml
    /// </summary>
    public partial class RunWindow : MetroWindow
    {
        private SineFaultModel faultModel;
        private Action<string> log;
        private ProgressDialogController progressController;

        public RunWindow(SineFaultModel faultModel, Action<string> logFunction)
        {
            InitializeComponent();
            
            this.log = logFunction;
            this.faultModel = faultModel;

            this.EnableDWMDropShadow = true;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            TestRunWorker testRunWorker = (TestRunWorker)faultModel.TestRunWorker;
            testRunWorker.Desired = (double)DesiredValueNumUpDown.Value;
            testRunWorker.Frequency = (double)FrequencyNumUpDown.Value;
            testRunWorker.RunWorkerCompleted += TestRunWorker_RunWorkerCompleted;

            progressController = await this.ShowProgressAsync("Please wait...", "Model simulation running");
            progressController.SetIndeterminate();
            testRunWorker.RunWorkerAsync(faultModel);

            log("Disturbance Fault Model - Ran model with desired value " + testRunWorker.Desired + " and frequency " + testRunWorker.Frequency);

        }

        private async void TestRunWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (progressController.IsOpen)
            {
                await progressController.CloseAsync();
            }

            if (((bool)e.Result) == false)
            {
                Exception exception = (this.faultModel.TestRunWorker as TestRunWorker).Exception;
                if (exception != null)
                {
                    log("Disturbance Fault Model - Test case failed to run.");
                    await this.ShowMessageAsync("Test case failed to run", "The test case run failed with error:\r\n\r\n" + exception.Message, MessageDialogStyle.Affirmative);
                }
                else
                {
                    log("Disturbance Fault Model - Test case failed to run due to a problem with the execution environment.");
                    await this.ShowMessageAsync("Test case failed to run", "Test case failed to run due to a problem with the execution environment.", MessageDialogStyle.Affirmative);
                }
            }
        }
    }
}
