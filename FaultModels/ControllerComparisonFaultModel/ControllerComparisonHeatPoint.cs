using FM4CC.Util.Heatmap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FM4CC.FaultModels.ControllerComparison
{
    internal class ControllerComparisonHeatPoint : HeatPoint
    {
        internal bool PhysicalRangeExceeded1 { get; set; }
        internal bool PhysicalRangeExceeded2 { get; set; }
        internal double WorstIntensity { get; set; }

        internal ControllerComparisonHeatPoint(double x, double y, double intensity, double worstIntensity, bool rangeExceeded1, bool rangeExceeded2) : base (x,y,intensity)
        {
            this.PhysicalRangeExceeded1 = rangeExceeded1;
            this.PhysicalRangeExceeded2 = rangeExceeded2;
            this.WorstIntensity = worstIntensity;
        }
    }
}
