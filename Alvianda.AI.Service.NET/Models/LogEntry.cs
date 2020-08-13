using System;

namespace Alvianda.AI.Service.CoreNet.Classes
{
    public class LogEntry
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
