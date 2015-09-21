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
    internal static class AllowedOscillationSineResultsParser
    {
        internal static void Parse(string explorationFile, ref List<AllowedOscillationInternalTestCase> testCases)
        {
            IEnumerable<string> lines = System.IO.File.ReadLines(explorationFile);
            testCases = new List<AllowedOscillationInternalTestCase>();

            if (lines == null) throw new ArgumentException("Invalid file");
            else
            {
                int cnt = 0;

                foreach (string line in lines)
                {
                    if (cnt++ == 0) continue;
                    string[] values = line.Split(',');

                    double initial = Double.Parse(values[0], CultureInfo.InvariantCulture);
                    int passed = 0;
                    bool isInt = Int32.TryParse(values[1], out passed);

                    if (isInt)
                    {
                        testCases.Add(new AllowedOscillationInternalTestCase(initial, initial, Convert.ToBoolean(passed), 5));
                    }

                    isInt = Int32.TryParse(values[2], out passed);

                    if (isInt)
                    {
                        testCases.Add(new AllowedOscillationInternalTestCase(initial, initial, Convert.ToBoolean(passed), 6));
                    }
                }
            }

        }
    }
}
