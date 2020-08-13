using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Alvianda.AI.Dashboard.Services
{
    public interface IWinePreparedataService
    {
        Task<Tuple<string, string>> ValidatePrepDataService();
        Task<Dictionary<string, string>> RunPrepDataAnalysis();
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

        public async Task<Tuple<string, string>> ValidatePrepDataService()
        {
            try
            {
                var serviceEndpoint = $"{_configuration.GetValue<string>("WinesetServiceAPI:BaseURI")}{_configuration.GetValue<string>("WinesetServiceAPI:AnalyticsRouting")}/validate";
                return await base.HttpGetRequest(serviceEndpoint);
            }
            catch (Exception ex)
            {
                return await new Task<Tuple<string, string>>(() => new Tuple<string, string>("error", ex.Message));
            }
        }

        public async Task<Dictionary<string, string>> RunPrepDataAnalysis()
        {
            var responseDictionary = new Dictionary<string, string>();
            try
            {
                var serviceEndpoint = $"{_configuration.GetValue<string>("WinesetServiceAPI:BaseURI")}{_configuration.GetValue<string>("WinesetServiceAPI:AnalyticsRouting")}/runanalyzer/dataset";
                var responseString = await base.HttpGetRequest(serviceEndpoint);

                IList<JToken> responseList = JsonConvert.DeserializeObject(responseString.Item2) as IList<JToken>;

                responseDictionary.Add("attributesHistogramTitle", responseList[1].Value<string>().Split(',')[0]);
                responseDictionary.Add("qualityHistogramTitle", responseList[1].Value<string>().Split(',')[1]);

                responseDictionary.Add("attributesHistogramChart", $"http:////localhost:53535//static//{responseList[0].Value<string>().Split(',')[0]}");
                responseDictionary.Add("qualityHistogramChart", $"http:////localhost:53535//static//{responseList[0].Value<string>().Split(',')[1]}");

                responseDictionary.Add("qualityValuesDropped", responseList[2].Value<string>());

                responseDictionary.Add("correlationChart", $"http:////localhost:53535//static//{responseList[3].Value<string>()}");
                responseDictionary.Add("correlationTitle", responseList[4].Value<string>());

                responseDictionary.Add("correlationAttributes", responseList[5].Value<string>());

                responseDictionary.Add("infomessage", responseList[6].Value<string>());

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
