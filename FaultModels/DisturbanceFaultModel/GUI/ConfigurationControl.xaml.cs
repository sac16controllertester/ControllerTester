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

namespace FM4CC.FaultModels.Disturbance.GUI
{
    /// <summary>
    /// Interaction logic for ConfigurationControl.xaml
    /// </summary>
    public partial class ConfigurationControl : UserControl, IConfigurationControl
    {
        DisturbanceFaultModelConfiguration configuration;
        SimulationSettings simulationSettings;

        public ConfigurationControl(FaultModelConfiguration configuration, SimulationSettings simulationSettings)
        {
            InitializeComponent();
            LoadConfiguration(configuration);
            this.simulationSettings = simulationSettings;

        }

        public bool Validate()
        {
            if ((double)this.DisturbanceDurationNumUpDown.Value > this.simulationSettings.ModelSimulationTime)
            {
                MessageBox.Show("Invalid setting", "The duration of the disturbance upon the actual value cannot be greater than the model simulation time!", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if ((double)this.DisturbanceDurationMaxNumUpDown.Value > this.DisturbanceDurationNumUpDown.Value)
            {
                MessageBox.Show("Invalid setting", "The duration of the disturbance upon the actual value cannot be greater than the model simulation time!", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

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

            Save();

            return true;
        }

        public void Save()
        {
            configuration.SetValue("DisturbanceSignalType", this.DisturbanceSignalTypeComboBox.SelectedIndex + 1);
            configuration.SetValue("DisturbanceDuration", this.DisturbanceDurationNumUpDown.Value);
            configuration.SetValue("DisturbanceUpTime", this.DisturbanceDurationMaxNumUpDown.Value);

            configuration.SetValue("Regions", Convert.ToDouble(Math.Abs((double)this.NumberHeatMapRegionsNumUpDown.Value)));
            configuration.SetValue("PointsPerRegion", Convert.ToDouble(Math.Abs((double)this.PointsPerRegionNumUpDown.Value)));

            configuration.SetValue("UseAdaptiveRandomSearch", ((string)this.ExplorationAlgorithmComboBox.SelectedItem) == "AdaptiveRandomSearch" ? true : false);
            configuration.SetValue("OptimizationAlgorithm", this.LocalSeachAlgorithmComboBox.SelectedItem);

            List<string> requirements = new List<string>();

            foreach (CheckBox cb in RequirementsStackPanel.Children)
            {
                if ((bool)cb.IsChecked) requirements.Add(cb.Content as string);
            }
            configuration.SetValue("Requirements", requirements, "complex");
        }

        private void LoadConfiguration(FaultModelConfiguration configuration)
        {
            this.configuration = (DisturbanceFaultModelConfiguration)configuration;

            this.ExplorationAlgorithmComboBox.Items.Add("AdaptiveRandomSearch");
            this.ExplorationAlgorithmComboBox.Items.Add("RandomSearch");

            this.LocalSeachAlgorithmComboBox.Items.Add("AcceleratedSimulatedAnnealing");
            this.LocalSeachAlgorithmComboBox.Items.Add("SimulatedAnnealing");
            this.LocalSeachAlgorithmComboBox.Items.Add("PatternSearch");
            this.LocalSeachAlgorithmComboBox.Items.Add("MultiStart");
            this.LocalSeachAlgorithmComboBox.Items.Add("GlobalSearch");
            this.LocalSeachAlgorithmComboBox.Items.Add("GeneticAlgorithm");

            this.DisturbanceSignalTypeComboBox.Items.Add("Trapezoidal Ramp");
            this.DisturbanceSignalTypeComboBox.Items.Add("Pulse");
            this.DisturbanceSignalTypeComboBox.Items.Add("Step");
            this.DisturbanceSignalTypeComboBox.Items.Add("Sine wave");
            this.DisturbanceSignalTypeComboBox.Items.Add("Constant");

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
                    default:
                        break;
                }
            }

            this.NumberHeatMapRegionsNumUpDown.Value = (double)configuration.GetValue("Regions") as double?;
            this.PointsPerRegionNumUpDown.Value = (double)configuration.GetValue("PointsPerRegion") as double?;

            this.DisturbanceSignalTypeComboBox.SelectedIndex = (int)(configuration.GetValue("DisturbanceSignalType")) - 1;
            this.DisturbanceDurationNumUpDown.Value = (double)configuration.GetValue("DisturbanceDuration") as double?;
            this.DisturbanceDurationMaxNumUpDown.Value = (double)configuration.GetValue("DisturbanceUpTime") as double?;

        }

        private void DisturbanceSignalTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedIndex != 0)
            {
                this.DisturbanceDurationMaxNumUpDown.IsEnabled = false;
                this.DisturbanceDurationMaxTextBlock.IsEnabled = false;
            }
            else
            {
                this.DisturbanceDurationMaxNumUpDown.IsEnabled = true;
                this.DisturbanceDurationMaxTextBlock.IsEnabled = true;
            }
        }

    }
}
