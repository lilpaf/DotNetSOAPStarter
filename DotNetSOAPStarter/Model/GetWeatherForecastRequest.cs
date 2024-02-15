using System.Xml.Serialization;

namespace DotNetSOAPStarter.Model
{
    public class GetWeatherForecastRequest
    {
        [XmlElement("value")]
        public int Value { get; set; }
    }
}