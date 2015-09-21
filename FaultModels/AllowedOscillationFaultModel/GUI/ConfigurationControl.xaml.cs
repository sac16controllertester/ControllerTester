using FM4CC.Environment;
using FM4CC.Simulation;
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
    /// Interaction logic for ConfigurationControl.xaml
    /// </summary>
    public partial class ConfigurationControl : UserControl, IConfigurationControl
    {
        AllowedOscillationFaultModelConfiguration configuration;
        SimulationSettings simulationSettings;

        public ConfigurationControl(FaultModelConfiguration configuration, SimulationSettings simulationSettings)
        {
            InitializeComponent();
            LoadConfiguration(configuration);
            this.simulationSettings = simulationSettings;

        }

        public bool Validate()
        {
            if (this.AllowedOscillationPercentageNumUpDown.Value != null && ((double)this.AllowedOscillationPercentageNumUpDown.Value) <= 0 && ((double)this.AllowedOscillationPercentageNumUpDown.Value) >= 100)
            {
                MessageBox.Show("Invalid setting", "Invalid allowed oscillation percentage", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (this.DesiredValueStepSizeNumUpDown.Value != null && ((double)this.DesiredValueStepSizeNumUpDown.Value) <= 0)
            {
                MessageBox.Show("Invalid setting", "Invalid desired value step size", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            Save();

            return true;
        }

        public void Save()
        {
            configuration.SetValue("DesiredValueStepSize", (double)this.DesiredValueStepSizeNumUpDown.Value);
            configuration.SetValue("AllowedOscillationPercentage", (double)this.AllowedOscillationPercentageNumUpDown.Value);
            configuration.SetValue("GenerateSineWaveTestCases", (bool)this.GenerateSineTestCasesCheckbox.IsChecked);
            configuration.SetValue("SineFrequency", (double)this.SineFrequencyNumUpDown.Value);
        }

        private void LoadConfiguration(FaultModelConfiguration configuration)
        {
            this.configuration = (AllowedOscillationFaultModelConfiguration)configuration;
            
            ReloadConfiguration();
        }

        public void ReloadConfiguration()
        {
            this.DesiredValueStepSizeNumUpDown.Value = (double)configuration.GetValue("DesiredValueStepSize") as double?;
            this.AllowedOscillationPercentageNumUpDown.Value = (double)configuration.GetValue("AllowedOscillationPercentage") as double?;
            this.GenerateSineTestCasesCheckbox.IsChecked = (bool)configuration.GetValue("GenerateSineWaveTestCases") as bool?;
            this.SineFrequencyNumUpDown.Value = (double)configuration.GetValue("SineFrequency") as double?;
        }
    }
}
