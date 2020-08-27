using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Alvianda.AI.Dashboard.Services
{
    public interface IWinePreparedataService
    {
        Task<Tuple<string, string>> ValidateGetPrepDataService();
        Task<Tuple<string, string>> ValidatePostPrepDataService();
        Task<Dictionary<string, string>> RunPrepDataAnalysis();
        Task<Dictionary<string, string>> PersistProcessedDataGet(string attributes,
                                                                    string userDescription = null, 
                                                                    string userNotes = null);
        Task<Dictionary<string, string>> PersistProcessedDataPost(string attributes,
                                                                    string userDescription = null, 
                                                                    string userNotes = null);
        //Task<List<WinesetEntry>> GetPaginatedResult(string logCategory, int currentPage, int pageSize = 10);
        //Task<int> GetCount(string wineCategory);
    }


    public class WinePreparedataService : BaseService, IWinePreparedataService
    {
        IConfiguration _configuration;

        public WinePreparedataService(HttpClient client,
                                    IConfiguration configuration) : base(client)
        {
            _configuration = configuration;
        }

        public async Task<Tuple<string, string>> ValidateGetPrepDataService()
        {
            try
            {
                var serviceEndpoint = $"{_configuration.GetValue<string>("WinesetServiceAPI:BaseURI")}{_configuration.GetValue<string>("WinesetServiceAPI:AnalyticsRouting")}/validate";
                var responseString = await HttpGetRequest(serviceEndpoint).ConfigureAwait(true);

                return new Tuple<string, string>("data", JsonConvert.DeserializeObject<string>(responseString.Item2));
            }
            catch (Exception ex)
            {
                return await new Task<Tuple<string, string>>(() => new Tuple<string, string>("error", ex.Message)).ConfigureAwait(true);
            }
        }

        public async Task<Tuple<string, string>> ValidatePostPrepDataService()
        {
            try
            {
                var serviceEndpoint = $"{_configuration.GetValue<string>("WinesetServiceAPI:BaseURI")}{_configuration.GetValue<string>("WinesetServiceAPI:AnalyticsRouting")}/validate";

#pragma warning disable CA2000 // Dispose objects before losing scope
                var testContent = new StringContent(JsonConvert.SerializeObject(new
                                            {
                                                scope = "WineAnalytics controller",
                                                action = "Validate POST method",
                                                result = "OK validation"
                                            }), Encoding.UTF8, "application/json");
#pragma warning restore CA2000 // Dispose objects before losing scope

                return await base.HttpPostRequest(serviceEndpoint,testContent).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                return await new Task<Tuple<string, string>>(() => new Tuple<string, string>("error", ex.Message)).ConfigureAwait(true);
            }
        }

        public async Task<Dictionary<string, string>> RunPrepDataAnalysis()
        {
            var responseDictionary = new Dictionary<string, string>();
            try
            {
                var serviceEndpoint = $"{_configuration.GetValue<string>("WinesetServiceAPI:BaseURI")}{_configuration.GetValue<string>("WinesetServiceAPI:AnalyticsRouting")}/runanalyzer/dataset/prepare";
                var responseString = await HttpGetRequest(serviceEndpoint).ConfigureAwait(true);

                IList<JToken> responseList = JsonConvert.DeserializeObject(responseString.Item2) as IList<JToken>;

                responseDictionary.Add("attributesHistogramTitle", responseList[1].Value<string>().Split(',')[0]);
                responseDictionary.Add("qualityHistogramTitle", responseList[1].Value<string>().Split(',')[1]);

                responseDictionary.Add("attributesHistogramChart", $"http://localhost:53535/static/{responseList[0].Value<string>().Split(',')[0]}");
                responseDictionary.Add("qualityHistogramChart", $"http://localhost:53535/static/{responseList[0].Value<string>().Split(',')[1]}");

                responseDictionary.Add("qualityValuesDropped", responseList[2].Value<string>());

                responseDictionary.Add("correlationChart", $"http://localhost:53535/static/{responseList[3].Value<string>()}");
                responseDictionary.Add("correlationTitle", responseList[4].Value<string>());

                responseDictionary.Add("correlationAttributes", responseList[5].Value<string>());

                responseDictionary.Add("infomessage", responseList[6].Value<string>());

                //responseDictionary.Add("preparredDataset", responseList[7].Value<string>());
                //responseDictionary.Add("fieldSet", responseList[8].Value<string>());

                //return await new Task<Dictionary<string,string>>(() => responseDictionary);
                return responseDictionary;

            }
            catch (Exception ex)
            {
                responseDictionary.Add("error", ex.Message);
                return responseDictionary;
            }
        }

        public async Task<Dictionary<string, string>> PersistProcessedDataGet(
                                                                string dataObjectAttributes,
                                                                string userNotes = null, 
                                                                string userDescription = null)
        {
            var responseDictionary = new Dictionary<string, string>();
            try
            {
                string description = string.IsNullOrEmpty(userDescription) ?  "[Default] Saved in database (persisted) the processed data objects" :  userDescription;
                string notes = string.IsNullOrEmpty(userNotes) ? "[Default] No notes entered by user." : userNotes;


                var serviceEndpoint = $"{_configuration.GetValue<string>("WinesetServiceAPI:BaseURI")}{_configuration.GetValue<string>("WinesetServiceAPI:AnalyticsRouting")}/runanalyzer/dataset/persist?description={description}&notes={userNotes}&attributes={dataObjectAttributes}";
                var responseString = await HttpGetRequest(serviceEndpoint).ConfigureAwait(true);

                //IList<JToken> responseList = JsonConvert.DeserializeObject(responseString.Item2) as IList<JToken>;

                responseDictionary.Add("infomessage", JsonConvert.DeserializeObject<string>(responseString.Item2));

                //return await new Task<Dictionary<string,string>>(() => responseDictionary);
                return responseDictionary;

            }
            catch (Exception ex)
            {
                responseDictionary.Add("error", ex.Message);
                return responseDictionary;
            }
        }

        public async Task<Dictionary<string, string>> PersistProcessedDataPost(
                                                                string dataObjectAttributes,
                                                                string userNotes = null,
                                                                string userDescription = null)
        {
            var responseDictionary = new Dictionary<string, string>();
            try
            {
                string description = string.IsNullOrEmpty(userDescription) ? "[Default] Saved in database (persisted) the processed data objects" : userDescription;
                string notes = string.IsNullOrEmpty(userNotes) ? "[Default] No notes entered by user." : userNotes;

                var dobjContent = new StringContent(JsonConvert.SerializeObject(new
                {
                    attributes = dataObjectAttributes,
                    description = userDescription,
                    notes = userNotes
                }), Encoding.UTF8, "application/json");

                var serviceEndpoint = $"{_configuration.GetValue<string>("WinesetServiceAPI:BaseURI")}{_configuration.GetValue<string>("WinesetServiceAPI:AnalyticsRouting")}/runanalyzer/dataset/persist?description={description}&notes={userNotes}&attributes={dataObjectAttributes}";
                var responseString = await HttpPostRequest(serviceEndpoint, dobjContent).ConfigureAwait(true);

                //IList<JToken> responseList = JsonConvert.DeserializeObject(responseString.Item2) as IList<JToken>;

                responseDictionary.Add("infomessage", JsonConvert.DeserializeObject<string>(responseString.Item2));

                //return await new Task<Dictionary<string,string>>(() => responseDictionary);
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
