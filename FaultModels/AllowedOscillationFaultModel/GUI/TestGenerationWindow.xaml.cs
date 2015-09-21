using FM4CC.Environment;
using FM4CC.ExecutionEngine;
using FM4CC.FaultModels;
using FM4CC.Simulation;
using FM4CC.TestCase;
using FM4CC.Util;
using FM4CC.Util.Heatmap;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
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

namespace FM4CC.FaultModels.AllowedOscillation.GUI
{
    /// <summary>
    /// Interaction logic for TestGenerationWindow.xaml
    /// </summary>
    public partial class TestGenerationWindow : MetroWindow
    {
        private AllowedOscillationFaultModel faultModel;
        private static ProgressDialogController progressController;
               
        private IList<TestCase.FaultModelTesterTestCase> testCases;
        private List<AllowedOscillationInternalTestCase> stepTopExplorationResults = null;
        private List<AllowedOscillationInternalTestCase> stepBottomExplorationResults = null;
        private List<AllowedOscillationInternalTestCase> sineExplorationResults = null;

        private Action<string> log;

        public TestGenerationWindow(FaultModel faultModel, IList<TestCase.FaultModelTesterTestCase> testCases, Action<string> logFunction)
        {
            InitializeComponent();

            this.log = logFunction;
            this.testCases = testCases;
            this.faultModel = faultModel as AllowedOscillationFaultModel;
            this.EnableDWMDropShadow = true;
            
            this.Title = "Test Case Generation - Allowed Oscillation Fault Model";
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

        private async void GenerationWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (progressController.IsOpen)
            {
                await progressController.CloseAsync();
            }

            if (((bool)e.Result) == true)
            {
                ProcessResults();
                log("Allowed Oscillation Fault Model - Search space exploration ended successfully");
            }
            else
            {
                Exception exception = (this.faultModel.SearchSpaceExplorationWorker as SearchSpaceExplorationWorker).Exception;

                if (exception != null)
                {
                    log("Allowed Oscillation Fault Model - Search space exploration failed to run.");
                    await this.ShowMessageAsync("Search space exploration failed", "The search space exploration failed with error:\r\n\r\n" + exception.Message, MessageDialogStyle.Affirmative);
                }
                else
                {
                    log("Allowed Oscillation Fault Model - Search space exploration stopped.");
                }
                this.Close();
            }
        }

        public void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            double progress = ((double)e.ProgressPercentage) / 100;
            if (progress > 1.00)
            {
                progress = 1.00;
            }

            progressController.SetProgress(progress);
            CheckIfCanceled();
        }

        private void ProcessResults()
        {
            string tempPath = Path.GetDirectoryName(faultModel.ExecutionInstance.GetValue("SUTPath")) + "\\ControllerTesterResults\\AllowedOscillation\\AllowedOscillation_StepResults.csv";

            AllowedOscillation.Parsers.AllowedOscillationStepResultsParser.Parse(tempPath, ref stepTopExplorationResults, ref stepBottomExplorationResults);
            if ((bool)faultModel.FaultModelConfiguration.GetValue("GenerateSineWaveTestCases"))
            {
                string sineTempPath = Path.GetDirectoryName(faultModel.ExecutionInstance.GetValue("SUTPath")) + "\\ControllerTesterResults\\AllowedOscillation\\AllowedOscillation_SineResults.csv";
                AllowedOscillation.Parsers.AllowedOscillationSineResultsParser.Parse(sineTempPath, ref sineExplorationResults);
            }

            this.DataContext = new TestGenerationViewModel(stepTopExplorationResults, stepBottomExplorationResults, sineExplorationResults);
            
        }
                
        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string tempPath = Path.GetDirectoryName(faultModel.ExecutionInstance.GetValue("SUTPath")) + "\\ControllerTesterResults\\AllowedOscillation\\AllowedOscillation_StepResults.csv";
            string sineTestCasesTempPath = Path.GetDirectoryName(faultModel.ExecutionInstance.GetValue("SUTPath")) + "\\ControllerTesterResults\\AllowedOscillation\\AllowedOscillation_SineResults.csv";
            
            bool generateSineTestCases = (bool)faultModel.FaultModelConfiguration.GetValue("GenerateSineWaveTestCases");
            
            // TODO add check for same oscillation and step size
            if (File.Exists(tempPath) && !generateSineTestCases && (MessageBoxResult.Yes == MessageBox.Show("Found test cases from a previous search space exploration. Do you want to load them instead?", "Previous exploration found", MessageBoxButton.YesNo, MessageBoxImage.Question)))
            {
                ProcessResults();            
            }
            else if (File.Exists(tempPath) && generateSineTestCases && File.Exists(sineTestCasesTempPath) && (MessageBoxResult.Yes == MessageBox.Show("Found test cases from a previous search space exploration. Do you want to load them instead?", "Previous exploration found", MessageBoxButton.YesNo, MessageBoxImage.Question)))
            {
                ProcessResults();
            }
            else
            {
                BackgroundWorker generationWorker = faultModel.SearchSpaceExplorationWorker;
                generationWorker.RunWorkerCompleted += GenerationWorker_RunWorkerCompleted;
                log("Allowed Oscillation Fault Model - Search space exploration started");

                progressController = await this.ShowProgressAsync("Performing search space exploration", "Estimated progress:", true);
                generationWorker.ProgressChanged += ProgressChanged;
                generationWorker.RunWorkerAsync(faultModel);
            }
        }

        private void CheckIfCanceled()
        {
            if (progressController.IsCanceled && faultModel.SearchSpaceExplorationWorker.IsBusy && !faultModel.SearchSpaceExplorationWorker.CancellationPending)
            {
                faultModel.SearchSpaceExplorationWorker.CancelAsync();
            }
        }
                
        private void RunButton_StepTop_Click(object sender, RoutedEventArgs e)
        {
            AllowedOscillationInternalTestCase internalTestCase = (AllowedOscillationInternalTestCase)StepTopExplorationDataGrid.SelectedValue;
            RunTestCase(internalTestCase);
        }

        private void RunButton_StepBottom_Click(object sender, RoutedEventArgs e)
        {
            AllowedOscillationInternalTestCase internalTestCase = (AllowedOscillationInternalTestCase)StepBottomExplorationDataGrid.SelectedValue;
            RunTestCase(internalTestCase);
        }

        private void RunButton_Sine_Click(object sender, RoutedEventArgs e)
        {
            AllowedOscillationInternalTestCase internalTestCase = (AllowedOscillationInternalTestCase)SineExplorationDataGrid.SelectedValue;
            RunTestCase(internalTestCase);
        }


        private async void RunTestCase(AllowedOscillationInternalTestCase internalTestCase)
        {
            TestRunWorker testRunWorker = (TestRunWorker)faultModel.TestRunWorker;
            testRunWorker.Desired = internalTestCase.InitialDesired;
            testRunWorker.TestCase = internalTestCase.TestCaseIndex;
            testRunWorker.RunWorkerCompleted += TestRunWorker_RunWorkerCompleted;

            progressController = await this.ShowProgressAsync("Running the model", "Please wait...");
            progressController.SetCancelable(false);
            progressController.SetIndeterminate();
            testRunWorker.RunWorkerAsync(faultModel);

            log("Allowed Oscillation Fault Model - Ran model with initial desired value " + internalTestCase.InitialDesired + ", test case " + internalTestCase.TestCaseIndex);

        }
                
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            bool saveTestCases = false;

            SaveTestCases(stepTopExplorationResults, ref saveTestCases);
            SaveTestCases(stepBottomExplorationResults, ref saveTestCases);
            SaveTestCases(sineExplorationResults, ref saveTestCases);

            if (saveTestCases)
            {
                this.DialogResult = true;
            }

            this.Close();

        }
   
        private void SaveTestCases(List<AllowedOscillationInternalTestCase> results, ref bool saveTestCases)
        {
            foreach (AllowedOscillationInternalTestCase testCase in results)
            {
                if (testCase.Save)
                {
                    saveTestCases = true;
                    AllowedOscillationFaultModelTestCase serializableTestCase = new AllowedOscillationFaultModelTestCase();
                    serializableTestCase.FaultModel = faultModel.ToString();
                    serializableTestCase.Name = testCase.InitialDesired + " " + testCase.Type;
                    serializableTestCase.Input = new SerializableDictionary<string, object>();
                    serializableTestCase.Input.Add("TestCaseType", testCase.TestCaseIndex);
                    serializableTestCase.Input.Add("Desired", testCase.InitialDesired);
                    testCases.Add(serializableTestCase);
                }
            }
        }
    }
}
