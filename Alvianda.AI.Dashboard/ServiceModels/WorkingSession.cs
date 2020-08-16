using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Alvianda.AI.Dashboard.ServiceModels
{
    public class WorkingSession
    {
        public string SessionId { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
