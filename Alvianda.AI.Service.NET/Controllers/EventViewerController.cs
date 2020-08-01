using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Alvianda.AI.Service.CoreNet.Classes;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using Alvianda.AI.Service.CoreNet.Extensions;
using Alvianda.AI.Service.CoreNet.Data;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using Alvianda.AI.Service.CoreNet.Models.SharedClasses;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Alvianda.AI.Service.CoreNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class EventViewerController : ControllerBase
    {
        private IConfiguration Configuration;
        private static int MAXRECORDS;
        private static int PAGESIZE;
        private IEventViewerConfiguration EventViewerSettings;
        public EventViewerController(IConfiguration configuration, IEventViewerConfiguration eventViewerSettings)
        {
            Configuration = configuration;

            if (MAXRECORDS == 0)
                MAXRECORDS = int.Parse(Configuration.GetValue<string>("LogReaderServiceSettings:MaxRecords"));
            if (PAGESIZE == 0)
                PAGESIZE = int.Parse(Configuration.GetValue<string>("LogReaderServiceSettings:PageSize"));

            EventViewerSettings = eventViewerSettings;
        }

        [HttpGet]
        [Route("logs/info")]
        public ActionResult<string> GetControllerInfo()
        {
            return Ok($"Web API - EventViewer Controller started on {DateTime.Now}");
        }

        [HttpGet]
        [Route("logs/capmaxrecs")]
        public ActionResult<int> GetCappedMaxRecs()
        {
            return Ok(MAXRECORDS);
        }

        [HttpGet]
        [Route("logs/categories")]
        [CbsRoleRequirement("COMPAgent")]
        [CbsPermissionRequirement("API.LogReader","AAD")]
        public ActionResult<IList<Log>> Logs()
        {
            var localMachine = System.Net.Dns.GetHostName();
            if (!EventViewerSettings.GetMachineName().Contains(localMachine))
                return Forbid();

            // return events recorded in the local EventViewer
            IList<Log> logs = new List<Log>();
            foreach (var log in System.Diagnostics.EventLog.GetEventLogs())
                logs.Add(new Log() { Name = log.Log, DisplayName = log.LogDisplayName });
            return Ok(logs);
        }

        [HttpGet("logs/records/{logName}/{pageNumber}")]
        [HttpGet("logs/records/{logName}/{pageNumber}/{maxRetrieved}")]
        [Route("logs/records")]
        [CbsRoleRequirement("COMPAgent")]
        //[CbsPermissionRequirement("API.LogReader","AAD")]
        [CbsPermissionRequirement("API.LogReader.LogEntriesPaged", "IDM")]
        public ActionResult<PaginatedList<LogEntry>> LogEntriesPaged(string logName, int pageNumber, int? maxRetrieved)
        {
            var localMachine = System.Net.Dns.GetHostName();
            if (!EventViewerSettings.GetMachineName().Contains(localMachine))
                return Forbid();

            EventLog myLog = new EventLog(logName);

            IList<EventLogEntry> myLogEntries = myLog.Entries.Cast<EventLogEntry>().ToList<EventLogEntry>();
            if (myLogEntries == null)
                return NotFound(null);

            IList<LogEntry> logEntries = new List<LogEntry>();
            int i = 0;
            foreach (EventLogEntry entry in myLogEntries)
            {
                logEntries.Add(new LogEntry()
                {
                    Id = i++,
                    TimeGenerated = entry.TimeGenerated,
                    Message = entry.Message,
                    MessageShort = entry.Message.Split('\r')[0],
                    Source = entry.Source,
                    InstanceId = entry.InstanceId,
                    UserName = entry.UserName,
                    MachineName = entry.MachineName
                });
                if (i >= MAXRECORDS)
                    break;
            }

            int pageSize = PAGESIZE;
            var paginatedResult = PaginatedList<LogEntry>.Create(logEntries, pageNumber, pageSize);

            if (maxRetrieved == null)
                paginatedResult.MaxRecords = MAXRECORDS;
            else
                paginatedResult.MaxRecords = (int)maxRetrieved;
            
            return Ok(paginatedResult);
        }
        
        [HttpGet("logs/records/{logName}/{fromDate}/{toDate}/{pageNumber}/{maxRetrieved}")]
        [HttpGet("logs/records/{logName}/{fromDate}/{toDate}/{pageNumber}")]
        [Route("logs/records")]
        [CbsRoleRequirement("COMPAgent")]
        [CbsPermissionRequirement("API.LogReader","AAD")]
        public ActionResult<PaginatedList<LogEntry>> DatedLogEntriesPaged(
                                                                string logName,
                                                                string fromDate,
                                                                string toDate,
                                                                int? pageNumber,
                                                                int? maxRetrieved)
        {
            var localMachine = System.Net.Dns.GetHostName();
            if (!EventViewerSettings.GetMachineName().Contains(localMachine))
                return Forbid();

            DateTime dateFrom;
            DateTime.TryParse(fromDate, out dateFrom);

            DateTime dateTo;
            DateTime.TryParse(toDate, out dateTo);

            EventLog myLog = new EventLog(logName);

            IList<EventLogEntry> myLogEntries = myLog.Entries.Cast<EventLogEntry>().ToList();

            if (myLogEntries == null)
                return NotFound(null);

            IList<LogEntry> logEntries = new List<LogEntry>();
            int i = 0;
            foreach (EventLogEntry entry in myLogEntries)
            {
                if (DateTime.Compare(entry.TimeGenerated, dateFrom) >= 0 && DateTime.Compare(entry.TimeGenerated, dateTo) <= 0)
                {
                    logEntries.Add(new LogEntry()
                    {
                        Id = i++,
                        TimeGenerated = entry.TimeGenerated,
                        Message = entry.Message,
                        MessageShort = entry.Message.Split('\r')[0],
                        Source = entry.Source,
                        InstanceId = entry.InstanceId,
                        UserName = entry.UserName,
                        MachineName = entry.MachineName
                    });
                    if (i >= MAXRECORDS)
                        break;
                }
            }

            int pageSize = PAGESIZE;
            var paginatedResult = PaginatedList<LogEntry>.Create(logEntries, pageNumber ?? 1, pageSize);

            //paginatedResult.MaxRecords = DATEDMAXRECORDS;
            if (maxRetrieved == null)
                paginatedResult.MaxRecords = MAXRECORDS;
            else
                paginatedResult.MaxRecords = (int)maxRetrieved;

            return Ok(paginatedResult);
        }

        [HttpGet("logs/categories/{fromDate}/{toDate}")]
        [Route("logs/categories")]
        [CbsRoleRequirement("COMPAgent")]
        [CbsPermissionRequirement("API.LogReader.Charts", "IDM")]
        public ActionResult<List<Tuple<Log,int>>> DatedLogsDistribution(
                                                                string fromDate,
                                                                string toDate)
        {
            var localMachine = System.Net.Dns.GetHostName();
            if (!EventViewerSettings.GetMachineName().Contains(localMachine))
                return Forbid();

            DateTime dateFrom;
            DateTime.TryParse(fromDate, out dateFrom);

            DateTime dateTo;
            DateTime.TryParse(toDate, out dateTo);

            IList<Log> logs = new List<Log>();
            foreach (var log in System.Diagnostics.EventLog.GetEventLogs())
            {
                if (fromDate == "null" && toDate=="null")
                    logs.Add(new Log() { Name = log.Log, DisplayName = log.LogDisplayName, EntriesCount = log.Entries.Count });
                else
                {
                    IList<EventLogEntry> logEntries = log.Entries.Cast<EventLogEntry>().ToList();
                    int count = 0;
                    foreach (var entry in logEntries)
                        if (DateTime.Compare(entry.TimeGenerated, dateFrom) >= 0 && DateTime.Compare(entry.TimeGenerated, dateTo) <= 0)
                            count++;
                    logs.Add(new Log() { Name = log.Log, DisplayName = log.LogDisplayName, EntriesCount = count });
                }
            }
            
            if (logs == null)
                return NotFound(null);

            return Ok(logs);
        }


        [HttpGet("logs/records/{logName}/keywords/{keywordList}/{pageNumber}/{maxRetrieved}")]
        [HttpGet("logs/records/{logName}/keywords/{keywordList}/{pageNumber}")]
        [HttpGet("logs/records/{logName}/keywords/{keywordList}")]
        [Route("logs/records")]
        [CbsRoleRequirement("COMPAgent")]
        [CbsPermissionRequirement("API.LogReader","AAD")]
        public ActionResult<PaginatedList<LogEntry>> KeywordsLogEntriesPaged(
                                                                string logName,
                                                                string keywordList,
                                                                int? pageNumber,
                                                                int? maxRetrieved)
        {
            var localMachine = System.Net.Dns.GetHostName();
            if (!EventViewerSettings.GetMachineName().Contains(localMachine))
                return Forbid();

            try
            {

                // validate keywordList to be comma separated
                // if not return 500 error
                IList<string> keywords;
                //var isValid = Regex.Match(keywordList, "^([a-zA-Z]+,)+[a-zA-Z]+$");
                var isValid = Regex.Match(keywordList, @"\w+(\s*,\s*\w+)*");
                if (isValid.Success)
                    keywords = keywordList.Split(',').ToList();
                else
                    return StatusCode(StatusCodes.Status500InternalServerError);

                EventLog myLog = new EventLog(logName);

                IList<EventLogEntry> myLogEntries = myLog.Entries.Cast<EventLogEntry>().ToList();

                if (myLogEntries == null)
                    return NotFound(null);

                IList<LogEntry> logEntries = new List<LogEntry>();
                int i = 0;
                foreach (EventLogEntry entry in myLogEntries)
                {
                    ObjectExtensions.Each(keywords, el =>
                    {
                        if (entry.Message.Contains(el) |
                                entry.Source.Contains(el) |
                                entry.MachineName.Contains(el) |
                                (string.IsNullOrEmpty(entry.UserName) ? false : entry.UserName.Contains(el)))
                        {
                            logEntries.Add(new LogEntry()
                            {
                                Id = i++,
                                TimeGenerated = entry.TimeGenerated,
                                Message = entry.Message.Replace(el, $"<mark style=\"background-color:yellow;\">{el}</mark>"),
                                MessageShort = entry.Message.Split('\r')[0].Replace(el, $"<mark style=\"background-color:yellow;\">{el}</mark>"),
                                Source = entry.Source.Replace(el, $"<mark style=\"background-color:yellow;\">{el}</mark>"),
                                InstanceId = entry.InstanceId,
                                UserName = entry.UserName == null ? entry.UserName : entry.UserName.Replace(el, $"<mark style=\"background-color:yellow;\">{el}</mark>"),
                                MachineName = entry.MachineName == null ? entry.MachineName : entry.MachineName.Replace(el, $"<mark style=\"background-color:yellow;\">{el}</mark>")
                            });
                        }
                    });
                    if (i >= MAXRECORDS)
                        break;
                }

                int pageSize = PAGESIZE;
                var paginatedResult = PaginatedList<LogEntry>.Create(logEntries, pageNumber ?? 1, pageSize);

                if (maxRetrieved == null)
                    paginatedResult.MaxRecords = MAXRECORDS;
                else
                    paginatedResult.MaxRecords = (int)maxRetrieved;

                if (logEntries.Count == 0)
                    return NotFound();

                return Ok(paginatedResult);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message,title: ex.Source);
            }
        }

        [HttpGet("logs/records/{logName}")]
        [Route("logs/records")]
        [CbsRoleRequirement("COMPAgent")]
        [CbsPermissionRequirement(claimValue:"API.LogReader",source:"AAD")]
        public ActionResult<IList<LogEntry>> LogEntries(string logName)
        {
            var localMachine = System.Net.Dns.GetHostName();
            if (!EventViewerSettings.GetMachineName().Contains(localMachine))
                return Forbid();

            IList<LogEntry> logEntries = new List<LogEntry>();
            EventLog myLog = new EventLog(logName);

            var myLogEntries = myLog.Entries.Cast<EventLogEntry>().ToList<EventLogEntry>();
            if (myLogEntries == null)
                return NotFound(null);

            int i = 0;
            foreach (EventLogEntry entry in myLogEntries)
            {
                logEntries.Add(new LogEntry()
                {
                    Id = i,
                    TimeGenerated = entry.TimeGenerated,
                    Message = entry.Message,
                    MessageShort = entry.Message.Split('\r')[0],
                    Source = entry.Source,
                    InstanceId = entry.InstanceId,
                    UserName = entry.UserName,
                    MachineName = entry.MachineName
                });
                i++;
                if (i > MAXRECORDS)
                    break;
            }
            return Ok(logEntries);
        }

        [HttpGet("logs/count/{logName}")]
        [Route("logs/count")]
        [CbsRoleRequirement("COMPAgent")]
        [CbsPermissionRequirement("API.LogReader","AAD")]
        public ActionResult<int> LogEntriesCount(string logName)
        {
            var localMachine = System.Net.Dns.GetHostName();
            if (!EventViewerSettings.GetMachineName().Contains(localMachine))
                return Forbid();

            EventLog myLog = new EventLog(logName);
            var count = myLog.Entries.Count;
            return Ok(count);
        }

        [HttpPost]
        [Route("logs/capmaxrecs")]
        public void Post([FromBody] CappedRecsSettings payload)
        {
            MAXRECORDS = int.Parse(payload.CappedMaxRecs);
        }

        [HttpPost]
        [Route("logs/machinename")]
        public void Post([FromBody] MachineSettings machineSettings)
        {
            EventViewerSettings.SetMachineName(machineSettings.MachineName);
        }
    }
}
