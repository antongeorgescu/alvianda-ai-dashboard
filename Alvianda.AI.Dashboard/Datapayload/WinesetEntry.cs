using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Alvianda.AI.Dashboard.Datapayload
{
    public class WinesetEntry
    {
        public int Id { get; set; }
        public double FixedAcidity { get; set; }
        public double VolatileAcidity { get; set; }
        public double CitricAcid { get; set; }
        public double ResidualSugar { get; set; }
        public double Chlorides { get; set; }
        public double FreeSulphurDioxide { get; set; }
        public double TotalSulphurDioxide { get; set; }
        public double Density { get; set; }
        public double PH { get; set; }
        public double Sulphates { get; set; }
        public double Alcohol { get; set; }
        public int Quality { get; set; }
    }
}
