using FM4CC.Environment;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FM4CC.FaultModels.AllowedOscillation
{
    public class AllowedOscillationFaultModelConfiguration : FaultModelConfiguration
    {
        private Dictionary<string, object> complexParameters;
        private Dictionary<string, object> primitiveParameters;

        public AllowedOscillationFaultModelConfiguration()
        {
            this.FaultModelName = "AllowedOscillationFaultModel";
            complexParameters = new Dictionary<string, object>();
            primitiveParameters = new Dictionary<string, object>();

            primitiveParameters.Add("DesiredValueStepSize", (double)0);
            primitiveParameters.Add("AllowedOscillationPercentage", (double)1);
            primitiveParameters.Add("GenerateSineWaveTestCases", (bool)true);
            primitiveParameters.Add("SineFrequency", (double)1);
        }

        public override object GetValue(string name, string collection = "primitives")
        {
            object value;

            if (collection == "primitives")
            {
                if (primitiveParameters.TryGetValue(name, out value))
                {
                    return value;
                }
                else
                {
                    throw new ArgumentException("Invalid parameter name for the Allowed Oscillation Fault Model - " + name);
                }
            }
            else throw new ArgumentException("Collection " + collection + " does not exist");         
        }

        public override void SetValue(string name, object value, string collection = "primitives")
        {
            if (collection == "primitives")
            {
                primitiveParameters[name] = value;
            }
            else throw new ArgumentException("Collection " + collection + " does not exist");
        }

        public override Dictionary<string, object>.Enumerator GetParametersEnumerator(string collection = "primitives")
        {
            if (collection == "primitives") return primitiveParameters.GetEnumerator();
            else throw new ArgumentException("Collection " + collection + " does not exist");
        }
                
        public override System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public override void ReadXml(XmlReader reader)
        {
            complexParameters.Clear();
            primitiveParameters.Clear();

            reader.MoveToContent();

            reader.ReadStartElement();

            primitiveParameters.Add("DesiredValueStepSize", reader.ReadElementContentAsDouble());
            primitiveParameters.Add("AllowedOscillationPercentage", reader.ReadElementContentAsDouble());
            primitiveParameters.Add("GenerateSineWaveTestCases", reader.ReadElementContentAsBoolean());
            primitiveParameters.Add("SineFrequency", reader.ReadElementContentAsDouble());

            reader.ReadEndElement();

        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("DesiredValueStepSize");
            writer.WriteValue((double)primitiveParameters["DesiredValueStepSize"]);
            writer.WriteEndElement();

            writer.WriteStartElement("AllowedOscillationPercentage");
            writer.WriteValue((double)primitiveParameters["AllowedOscillationPercentage"]);
            writer.WriteEndElement();

            writer.WriteStartElement("GenerateSineWaveTestCases");
            writer.WriteValue((bool)primitiveParameters["GenerateSineWaveTestCases"]);
            writer.WriteEndElement();

            writer.WriteStartElement("SineFrequency");
            writer.WriteValue((double)primitiveParameters["SineFrequency"]);
            writer.WriteEndElement();
        }
    }
}
