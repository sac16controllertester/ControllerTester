using FM4CC.Util.Heatmap;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FM4CC.FaultModels.Sine
{
    internal class DataGridHeatPoint : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private double _baseUnit;
        private double _baseUnitFrequency;
        private SineHeatPoint _containedHeatPoint;
        private bool _analyze;
        private double _desiredMin;
        private double _desiredMax;
        private double _frequencyMin;
        private double _frequencyMax;

        public double WorstIntensity
        {
            get
            {
                return _containedHeatPoint.WorstIntensity;
            }
        }

        public bool PhysicalRangeExceeded 
        {
            get
            {
                return _containedHeatPoint.PhysicalRangeExceeded;
            }
        }

        public HeatPoint ContainedHeatPoint
        {
            get
            {
                return _containedHeatPoint;
            }
        }

        public bool Analyze
        {
            get
            {
                return _analyze;
            }
            set
            {
                _analyze = value;
                RaisePropertyChanged("Analyze");
            }
        }
        public double Intensity 
        { 
            get 
            {
                return _containedHeatPoint.Intensity;
            }
        }

        public int DesiredRegion
        {
            get
            {
                if (_containedHeatPoint.Y == _desiredMin)
                {
                    return 0;
                }
                else if (_containedHeatPoint.Y == _desiredMax)
                {
                    return (int)((_containedHeatPoint.Y - _desiredMin) / _baseUnit) - 1;
                }
                else
                {
                    return (int)((_containedHeatPoint.Y - _desiredMin) / _baseUnit);
                }
            }
        }

        public int FrequencyRegion 
        {
            get
            {
                if (_containedHeatPoint.X == _frequencyMin)
                {
                    return 0;
                }
                else if (_containedHeatPoint.X == _frequencyMax)
                {
                    return (int)((_containedHeatPoint.X - _frequencyMin) / _baseUnitFrequency) - 1;
                }
                else
                {
                    return (int)((_containedHeatPoint.X - _frequencyMin) / _baseUnitFrequency);
                }
            }
        }

        public double Desired 
        { 
            get
            {
                return _containedHeatPoint.Y;
            }
        }

        public double Frequency 
        { 
            get
            {
                return _containedHeatPoint.X;
            }
        }

        public string Requirement { get; set; }

        public double BaseUnit
        {
            get
            {
                return _baseUnit;
            }
        }

        public double BaseUnitFrequency
        {
            get
            {
                return _baseUnitFrequency;
            }
        }

        public DataGridHeatPoint(SineHeatPoint hp, double baseUnit, double baseUnitFrequency, double desiredMin, double desiredMax, double frequencyMin, double frequencyMax, string requirement)
        {
            this.Requirement = requirement;
            
            _baseUnit = baseUnit;
            _baseUnitFrequency = baseUnitFrequency;
            _containedHeatPoint = hp;
            _analyze = true;

            _desiredMin = desiredMin;
            _desiredMax = desiredMax;
            _frequencyMin = frequencyMin;
            _frequencyMax = frequencyMax;
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
