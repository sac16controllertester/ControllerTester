using FM4CC.Util.Heatmap;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FM4CC.FaultModels.AllowedOscillation
{
    internal class AllowedOscillationInternalTestCase : INotifyPropertyChanged
    {
        private bool _save;

        public event PropertyChangedEventHandler PropertyChanged;
        public bool Save
        {
            get
            {
                return _save;
            }
            set
            {
                _save = value;
                RaisePropertyChanged("Save");
            }
        }
        public double InitialDesired { get; set; }
        public double FinalDesired { get; set; }
        public bool Passed { get; set; }
        public string Type { get; set; }
        public int TestCaseIndex { get; set; }
        public bool PossibleStabilityProblems { get; set; }

        public AllowedOscillationInternalTestCase(double initial, double final, bool passed, int index, bool smellPresent = false)
        {
            this.Save = false;
            this.InitialDesired = initial;
            this.FinalDesired = final;
            this.Passed = passed;
            this.TestCaseIndex = index;
            this.PossibleStabilityProblems = smellPresent;

            switch(index)
            {
                case 1:
                    this.Type = "Top Limit, not allowed";
                    break;
                case 2:
                    this.Type = "Bottom Limit, not allowed";
                    break;
                case 3:
                    this.Type = "Top Limit, allowed";
                    break;
                case 4:
                    this.Type = "Bottom Limit, allowed";
                    break;
                case 5:
                    this.Type = "Sine wave, allowed";
                    break;
                case 6:
                    this.Type = "Sine wave, not allowed";
                    break;
                default:
                    throw new ArgumentException("Invalid test case index.");
            }
        }

        #region Methods

        private void RaisePropertyChanged(string propertyName)
        {
            // take a copy to prevent thread issues
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
