using FM4CC.Environment;
using FM4CC.Simulation;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace FM4CC.FaultModels.ControllerComparison.GUI
{
    /// <summary>
    /// Interaction logic for ConfigurationControl.xaml
    /// </summary>
    public partial class ConfigurationControl : UserControl, IConfigurationControl
    {
        ControllerComparisonFaultModelConfiguration configuration;
        SimulationSettings simulationSettings;

        public ConfigurationControl(FaultModelConfiguration configuration, SimulationSettings simulationSettings)
        {
            InitializeComponent();
            LoadConfiguration(configuration);
            this.simulationSettings = simulationSettings;

        }

        public bool Validate()
        {
            if (this.NumberHeatMapRegionsNumUpDown.Value != null && ((double)this.NumberHeatMapRegionsNumUpDown.Value) <= 0)
            {
                MessageBox.Show("Invalid setting", "Invalid heat map divison factor", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (this.PointsPerRegionNumUpDown.Value != null && ((double)this.PointsPerRegionNumUpDown.Value) <= 0)
            {
                MessageBox.Show("Invalid setting", "Invalid no. of points per region", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if ((!ModelPathTextBox.Text.Contains(".mdl") && !ModelPathTextBox.Text.Contains(".slx")) || !File.Exists(this.ModelPathTextBox.Text))
            {
                MessageBox.Show("Invalid model path, expected a MATLAB model file (.mdl, .slx)", "Model missing", MessageBoxButton.OK, MessageBoxImage.Stop);
                return false;
            }

            if (!ModelSettingsPathTextBox.Text.Contains(".m") || !File.Exists(this.ModelSettingsPathTextBox.Text))
            {
                MessageBox.Show("Invalid model settings path, expected a MATLAB script file (.m)", "Model settings missing", MessageBoxButton.OK, MessageBoxImage.Stop);
                return false;
            }

            if (this.DesiredValueNameTextBox.Text == null || this.DesiredValueNameTextBox.Text == "")
            {
                MessageBox.Show("Please specify the name of the desired variable.", "Invalid setting", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (this.ActualValueNameTextBox.Text == null || this.ActualValueNameTextBox.Text == "")
            {
                MessageBox.Show("Please specify the name of the actual variable.", "Invalid setting", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            Save();

            return true;
        }

        public void Save()
        {
            configuration.SetValue("Regions", Convert.ToDouble(Math.Abs((double)this.NumberHeatMapRegionsNumUpDown.Value)));
            configuration.SetValue("PointsPerRegion", Convert.ToDouble(Math.Abs((double)this.PointsPerRegionNumUpDown.Value)));

            configuration.SetValue("UseAdaptiveRandomSearch", ((string)this.ExplorationAlgorithmComboBox.SelectedItem) == "AdaptiveRandomSearch" ? true : false);
            configuration.SetValue("OptimizationAlgorithm", this.LocalSeachAlgorithmComboBox.SelectedItem);

            configuration.SetValue("Compared_ModelFile", this.ModelPathTextBox.Text);
            configuration.SetValue("Compared_ModelConfigurationFile", this.ModelSettingsPathTextBox.Text);
            configuration.SetValue("Compared_DesiredVariableName", this.DesiredValueNameTextBox.Text);
            configuration.SetValue("Compared_ActualVariableName", this.ActualValueNameTextBox.Text);

            if (this.DisturbanceValueNameTextBox.Text != null)
            {
                configuration.SetValue("Compared_DisturbanceVariableName", this.DisturbanceValueNameTextBox.Text);
            }
            else
            {
                configuration.SetValue("Compared_DisturbanceVariableName", "");
            }

            List<string> requirements = new List<string>();

            int i = 0;

            foreach (CheckBox cb in RequirementsStackPanel.Children)
            {
                if (i > 1)
                {
                    if ((bool)cb.IsChecked) requirements.Add(cb.Content as string);
                }
                else
                {
                    if (i == 0 && ((bool)cb.IsChecked))
                    {
                        requirements.Add("MaxDeviation");
                    }
                    else if ((bool)cb.IsChecked)
                    {
                        requirements.Add("MeanDeviation");
                    }
                }

                i++;
            }
            configuration.SetValue("Requirements", requirements, "complex");

        }

        private void LoadConfiguration(FaultModelConfiguration configuration)
        {
            this.configuration = (ControllerComparisonFaultModelConfiguration)configuration;

            this.ExplorationAlgorithmComboBox.Items.Add("AdaptiveRandomSearch");
            this.ExplorationAlgorithmComboBox.Items.Add("RandomSearch");

            this.LocalSeachAlgorithmComboBox.Items.Add("AcceleratedSimulatedAnnealing");
            this.LocalSeachAlgorithmComboBox.Items.Add("SimulatedAnnealing");
            this.LocalSeachAlgorithmComboBox.Items.Add("PatternSearch");
            this.LocalSeachAlgorithmComboBox.Items.Add("MultiStart");
            this.LocalSeachAlgorithmComboBox.Items.Add("GlobalSearch");
            this.LocalSeachAlgorithmComboBox.Items.Add("GeneticAlgorithm");

            ReloadConfiguration();
        }

        public void ReloadConfiguration()
        {
            List<string> requirements = configuration.GetValue("Requirements", "complex") as List<string>;

            this.ExplorationAlgorithmComboBox.SelectedIndex = ((bool)configuration.GetValue("UseAdaptiveRandomSearch"))?0:1;
            this.LocalSeachAlgorithmComboBox.SelectedValue = (string)configuration.GetValue("OptimizationAlgorithm");

            foreach (string requirement in requirements)
            {
                switch (requirement)
                {
                    case "Stability":
                        StabilityCheckBox.IsChecked = true;
                        break;
                    case "Precision":
                        PrecisionCheckBox.IsChecked = true;
                        break;
                    case "Smoothness":
                        SmoothnessCheckBox.IsChecked = true;
                        break;
                    case "Responsiveness":
                        ResponsivenessCheckBox.IsChecked = true;
                        break;
                    case "Steadiness":
                        SteadinessCheckBox.IsChecked = true;
                        break;
                    case "MaxDeviation":
                        MaxDeviationCheckBox.IsChecked = true;
                        break;
                    case "MeanDeviation":
                        MeanDeviationCheckBox.IsChecked = true;
                        break;
                    default:
                        break;
                }
            }

            this.NumberHeatMapRegionsNumUpDown.Value = (double)configuration.GetValue("Regions") as double?;
            this.PointsPerRegionNumUpDown.Value = (double)configuration.GetValue("PointsPerRegion") as double?;

            this.ModelPathTextBox.Text = (string)configuration.GetValue("Compared_ModelFile");
            this.ModelSettingsPathTextBox.Text = (string)configuration.GetValue("Compared_ModelConfigurationFile");
            this.DesiredValueNameTextBox.Text = (string)configuration.GetValue("Compared_DesiredVariableName");
            this.ActualValueNameTextBox.Text = (string)configuration.GetValue("Compared_ActualVariableName");
            this.DisturbanceValueNameTextBox.Text = (string)configuration.GetValue("Compared_DisturbanceVariableName");
        }

        private void ModelBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fbd = new OpenFileDialog();
            fbd.ShowDialog();
            this.ModelPathTextBox.Text = fbd.FileName;
        }

        private void ModelSettingsBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fbd = new OpenFileDialog();
            fbd.ShowDialog();
            this.ModelSettingsPathTextBox.Text = fbd.FileName;
        }
    }
}
