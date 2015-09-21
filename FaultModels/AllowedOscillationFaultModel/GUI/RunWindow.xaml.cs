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

namespace FM4CC.FaultModels.AllowedOscillation.GUI
{
    /// <summary>
    /// Interaction logic for RunWindow.xaml
    /// </summary>
    public partial class RunWindow : MetroWindow
    {
        private AllowedOscillationFaultModel faultModel;
        private ProgressDialogController progressController;
        private Action<string> log;

        public RunWindow(AllowedOscillationFaultModel faultModel, Action<string> logFunction)
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
            testRunWorker.TestCase = TestCaseComboBox.SelectedIndex + 1;
            testRunWorker.RunWorkerCompleted += TestRunWorker_RunWorkerCompleted;
                        
            progressController = await this.ShowProgressAsync("Please wait...", "Model simulation running");
            progressController.SetIndeterminate();

            testRunWorker.RunWorkerAsync(faultModel);

            log("Allowed Oscillation Fault Model - Ran the model with desired value " + testRunWorker.Desired + " and test case" + testRunWorker.TestCase);
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
                    log("Allowed Oscillation Fault Model - Test case failed to run.");
                    await this.ShowMessageAsync("Test case failed to run", "The test case run failed with error:\r\n\r\n" + exception.Message, MessageDialogStyle.Affirmative);
                }
                else
                {
                    log("Allowed Oscillation Fault Model - Test case did not pass.");
                    await this.ShowMessageAsync("Test case did not pass", "The test case did not pass, please check the plot.", MessageDialogStyle.Affirmative);
                }
            }
        }
    }
}
