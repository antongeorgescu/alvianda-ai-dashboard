using Alvianda.AI.Dashboard.ServiceModels;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUglify.Helpers;
using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Alvianda.AI.Dashboard.Services
{
    public interface IWineMlmodelService
    {
        Task<Tuple<string, IList<Algorithm>,string>> GetAlgorithmList(string algorithmType);
        Task<Tuple<string, IList<WorkingSession>, string>> GetWorkingSessionList(int applicationId);
        Task<Dictionary<string, string>> RunMachineLearningModel(string algorithm, string SessionId, string Decsription, string Notes);
        //Task<List<WinesetEntry>> GetPaginatedResult(string logCategory, int currentPage, int pageSize = 10);
        //Task<int> GetCount(string wineCategory);
    }

    

    public class WineMlmodelService : BaseService, IWineMlmodelService
    {
        IConfiguration _configuration;

        public WineMlmodelService(HttpClient client,
                                    IConfiguration configuration) : base(client)
        {
            _configuration = configuration;
        }

        public async Task<Tuple<string, IList<Algorithm>,string>> GetAlgorithmList(string algorithmType)
        {
            try
            {
                var serviceEndpoint = $"{_configuration.GetValue<string>("WinesetServiceAPI:BaseURI")}{_configuration.GetValue<string>("WinesetServiceAPI:AnalyticsRouting")}/algorithms?type={algorithmType}";
                var responseString = await HttpGetRequest(serviceEndpoint).ConfigureAwait(true);
                IList<JToken> responseList = JsonConvert.DeserializeObject(responseString.Item2) as IList<JToken>;
                
                IList<Algorithm> algorithms = new List<Algorithm>();
                responseList.ForEach(x => algorithms.Add(new Algorithm()
                {
                    Id = int.Parse(x["Id"].ToString()),
                    Name = x["Name"].ToString(),
                    DisplayName = x["DisplayName"].ToString(),
                    Description = x["Description"].ToString()
                }));
                return new Tuple<string, IList<Algorithm>, string>(item1: "info", item2: algorithms,item3:string.Empty);
            }
            catch (Exception ex)
            {
                return await new Task<Tuple<string, IList<Algorithm>,string>>(() => new Tuple<string, IList<Algorithm>, string> ("error", null,ex.Message)).ConfigureAwait(true);
            }
        }

        public async Task<Tuple<string, IList<WorkingSession>, string>> GetWorkingSessionList(int applicationId)
        {
            try
            {
                var serviceEndpoint = $"{_configuration.GetValue<string>("WinesetServiceAPI:BaseURI")}{_configuration.GetValue<string>("WinesetServiceAPI:AnalyticsRouting")}/worksessions?applicationid={applicationId}";
                var responseString = await HttpGetRequest(serviceEndpoint).ConfigureAwait(true);
                IList<JToken> responseList = JsonConvert.DeserializeObject(responseString.Item2) as IList<JToken>;

                IList<WorkingSession> workSessions = new List<WorkingSession>();
                responseList.ForEach(x => workSessions.Add(new WorkingSession()
                {
                    SessionId = x["SessionId"].ToString(),
                    Description = x["Description"].ToString(),
                    CreatedOn = DateTime.Parse(x["CreatedOn"].ToString())
                }));
                return new Tuple<string, IList<WorkingSession>, string>(item1: "info", item2: workSessions, item3: string.Empty);
            }
            catch (Exception ex)
            {
                return await new Task<Tuple<string, IList<WorkingSession>, string>>(() => new Tuple<string, IList<WorkingSession>, string>("error", null, ex.Message)).ConfigureAwait(true);
            }
        }

        public async Task<Tuple<string, JToken, string>> GetSessionDetails(string sessionId)
        {
            try
            {
                var serviceEndpoint = $"{_configuration.GetValue<string>("WinesetServiceAPI:BaseURI")}{_configuration.GetValue<string>("WinesetServiceAPI:AnalyticsRouting")}/worksessions/details?sessionid={sessionId}";
                var responseString = await HttpGetRequest(serviceEndpoint).ConfigureAwait(true);
                var jsonDetails = JToken.Parse(responseString.Item2);
                
                return new Tuple<string, JToken, string>(item1: "info", item2: jsonDetails, item3: string.Empty);
            }
            catch (Exception ex)
            {
                return await new Task<Tuple<string, JToken, string>>(() => new Tuple<string, JToken, string>("error", null, ex.Message)).ConfigureAwait(true);
            }
        }

        public async Task<Tuple<string, IList<DataObjectPersisted>, string>> GetSavedDataObjectList(int applicationId)
        {
            try
            {
                var serviceEndpoint = $"{_configuration.GetValue<string>("WinesetServiceAPI:BaseURI")}{_configuration.GetValue<string>("WinesetServiceAPI:AnalyticsRouting")}/saveddataobjects?applicationid={applicationId}";
                var responseString = await HttpGetRequest(serviceEndpoint).ConfigureAwait(true);
                IList<JToken> responseList = JsonConvert.DeserializeObject(responseString.Item2) as IList<JToken>;

                IList<DataObjectPersisted> dobjs = new List<DataObjectPersisted>();
                responseList.ForEach(x => dobjs.Add(new DataObjectPersisted()
                {
                    ApplicationId = int.Parse(x["ApplicationId"].ToString()),
                    SessionId = x["SessionId"].ToString(),
                    DONames = x["DONames"].ToString().Split(',').ToList<string>(),
                    DODescriptions = x["DODescriptions"].ToString().Split(',').ToList<string>(),
                    CreatedDates = x["CreatedDates"].ToString().Split(',').ToList<string>()
                }));
                return new Tuple<string, IList<DataObjectPersisted>, string>(item1: "info", item2: dobjs, item3: string.Empty);
            }
            catch (Exception ex)
            {
                return await new Task<Tuple<string, IList<DataObjectPersisted>, string>>(() => new Tuple<string, IList<DataObjectPersisted>, string>("error", null, ex.Message)).ConfigureAwait(true);
            }
        }

        public async Task<Dictionary<string, string>> RunMachineLearningModel(
                                                                        string algorithm, 
                                                                        string sessionId,
                                                                        string description,
                                                                        string notes)
        {
            var responseDictionary = new Dictionary<string, string>();
            try
            {
                var serviceEndpoint = $"{_configuration.GetValue<string>("WinesetServiceAPI:BaseURI")}{_configuration.GetValue<string>("WinesetServiceAPI:AnalyticsRouting")}/runanalyzer/trainmodel";
                var paramContent = new StringContent(JsonConvert.SerializeObject(new
                {
                    algorithm = algorithm,
                    sessionid = sessionId,
                    description = description,
                    notes = notes
                }), Encoding.UTF8, "application/json");   
                var responseString = await HttpPostRequest(serviceEndpoint, paramContent).ConfigureAwait(true);
                var jsonDetails = JToken.Parse(responseString.Item2);
                var modelPerformance = jsonDetails.Value<string>().Split('|')[0];
                var confusionMatrixArray = jsonDetails.Value<string>().Split('|')[1].Split("\n");
                var modelId = jsonDetails.Value<string>().Split('|')[2];

                responseDictionary.Add("Model Accuracy", modelPerformance);
                responseDictionary.Add("Confusion Matrix", string.Join(",",confusionMatrixArray));
                responseDictionary.Add("Model Id", modelId);

                return responseDictionary;

            }
            catch (Exception ex)
            {
                responseDictionary.Add("error", ex.Message);
                return responseDictionary;
            }
        }
    }
}
