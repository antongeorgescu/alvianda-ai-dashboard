using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Alvianda.AI.Service.CoreNet.Data
{
    public interface IEventViewerConfiguration {
        public void SetMachineName(string name);
        public string GetMachineName();
    }
    public class EventViewerConfiguration : IEventViewerConfiguration
    {
        private string _machineName;
        public void SetMachineName(string name)
        {
            _machineName = name;
        }

        public string GetMachineName()
        {
            return _machineName;
        }
    }
}
