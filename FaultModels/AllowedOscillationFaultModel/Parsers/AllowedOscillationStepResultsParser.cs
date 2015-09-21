using FM4CC.FaultModels.AllowedOscillation;
using FM4CC.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FM4CC.FaultModels.AllowedOscillation.Parsers
{
    internal static class AllowedOscillationStepResultsParser
    {
        internal static void Parse(string explorationFile, ref List<AllowedOscillationInternalTestCase> topLimitTestCases, ref List<AllowedOscillationInternalTestCase> bottomLimitTestCases)
        {
            topLimitTestCases = new List<AllowedOscillationInternalTestCase>();
            bottomLimitTestCases = new List<AllowedOscillationInternalTestCase>();
            IEnumerable<string> lines = System.IO.File.ReadLines(explorationFile);

            if (lines == null) throw new ArgumentException("Invalid file");
            else
            {
                int cnt = 0;

                foreach (string line in lines)
                {
                    if (cnt++ == 0) continue;
                    string[] values = line.Split(',');

                    double initial = Double.Parse(values[0], CultureInfo.InvariantCulture);
                    double[] final = { Double.Parse(values[1], CultureInfo.InvariantCulture), Double.Parse(values[2], CultureInfo.InvariantCulture), Double.Parse(values[3], CultureInfo.InvariantCulture), Double.Parse(values[4], CultureInfo.InvariantCulture) };

                    for (int i = 0; i < 4; i++)
                    {
                        int passed = 0;
                        bool isInt = Int32.TryParse(values[i+5], out passed);

                        if (isInt)
                        {
                            int smell = 0;
                            isInt = Int32.TryParse(values[i + 9], out smell);

                            if (isInt)
                            {
                                if (i % 2 == 0)
                                {
                                    topLimitTestCases.Add(new AllowedOscillationInternalTestCase(initial, final[i], Convert.ToBoolean(passed), 1 + i, Convert.ToBoolean(smell)));
                                }
                                else
                                {
                                    bottomLimitTestCases.Add(new AllowedOscillationInternalTestCase(initial, final[i], Convert.ToBoolean(passed), 1 + i, Convert.ToBoolean(smell)));
                                }
                            }
                        }
                    }

                }
            }

        }
    }
}
