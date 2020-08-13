using System;

namespace Alvianda.AI.Dashboard.Datapayload
{
    public class EventViewerEntry
    {
        public int Id { get; set; }
        public long InstanceId { get; set; }
        public DateTime TimeGenerated { get; set; }
        public string Source { get; set; }

        public string MessageShort { get; set; }
        public string Message { get; set; }
        public string UserName { get; set; }
        public string MachineName { get; set; }
    }
}
