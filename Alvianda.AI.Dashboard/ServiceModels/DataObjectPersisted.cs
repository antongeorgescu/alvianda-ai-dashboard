using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Alvianda.AI.Dashboard.ServiceModels
{
    public class DataObjectPersisted
    {
        public string SessionId { get; set; }
        public int ApplicationId { get; set; }
        public IList<string> DONames { get; set; }
        public IList<string> DODescriptions { get; set; }
        public IList<string> CreatedDates { get; set; }
    }
}
