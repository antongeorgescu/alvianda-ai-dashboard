using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Alvianda.AI.Dashboard.Datapayload
{
    public class WinesetEntry
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("fixed acidity")]
        public double FixedAcidity { get; set; }
        [JsonPropertyName("volatile acidity")]
        public double VolatileAcidity { get; set; }
        [JsonPropertyName("citric acid")]
        public double CitricAcid { get; set; }
        [JsonPropertyName("residual sugar")]
        public double ResidualSugar { get; set; }
        [JsonPropertyName("chlorides")]
        public double Chlorides { get; set; }
        [JsonPropertyName("free sulphur dioxide")]
        public double FreeSulphurDioxide { get; set; }
        [JsonPropertyName("totla sulphur dioxide")]
        public double TotalSulphurDioxide { get; set; }
        [JsonPropertyName("density")]
        public double Density { get; set; }
        [JsonPropertyName("pH")]
        public double PH { get; set; }
        [JsonPropertyName("sulphates")]
        public double Sulphates { get; set; }
        [JsonPropertyName("alcohol")]
        public double Alcohol { get; set; }
        [JsonPropertyName("quality")]
        public int Quality { get; set; }
    }
}
