﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Alvianda.AI.Dashboard.Data
{
    public interface IEventViewerMachineListConfig
    {
        public IList<EventViewerMachine> MachineList();
    }

    public class EventViewerMachineListConfig : IEventViewerMachineListConfig
    {
        private readonly IConfiguration Configuration;
        public EventViewerMachineListConfig(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IList<EventViewerMachine> MachineList()
        {
            var machineList = new List<EventViewerMachine>();
            var configMachineList = Configuration.GetSection("LoggerServicesAPI:EventViewerHostMachines").Get<string[]>();
            foreach (var item in configMachineList)
                machineList.Add(new EventViewerMachine() { Name = item.Split('|')[0], Description = item.Split('|')[1] });

            return machineList;
        }
    }
    
    public class EventViewerMachine
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
