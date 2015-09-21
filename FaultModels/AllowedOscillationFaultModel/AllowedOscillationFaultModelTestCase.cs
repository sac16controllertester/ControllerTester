using FM4CC.TestCase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FM4CC.FaultModels.AllowedOscillation
{
    public class AllowedOscillationFaultModelTestCase : FaultModelTesterTestCase
    {
        public override string GetDescription()
        {
            string type = null;
            switch ((int)Input["TestCaseType"])
            {
                case 1:
                    type = "Top Limit, not allowed";
                    break;
                case 2:
                    type = "Bottom Limit, not allowed";
                    break;
                case 3:
                    type = "Top Limit, allowed";
                    break;
                case 4:
                    type = "Bottom Limit, allowed";
                    break;
                case 5:
                    type = "Sine wave, allowed";
                    break;
                case 6:
                    type = "Sine wave, not allowed";
                    break;
                default:
                    throw new ArgumentException("Invalid test case index.");
            }
            return "Desired Value - " + Input["Desired"] + ", test case type - " + type + "\r\nPassed - " + this.Passed;
        }
    }
}
