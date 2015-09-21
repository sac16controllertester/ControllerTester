using FM4CC.TestCase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FM4CC.FaultModels.Sine
{
    public class SineFaultModelTestCase : FaultModelTesterTestCase
    {
        public override string GetDescription()
        {
            return "Desired Value - " + Input["Desired"] + ", Frequency " + Input["Frequency"];
        }
    }
}
