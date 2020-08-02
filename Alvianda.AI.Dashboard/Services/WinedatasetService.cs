using Alvianda.AI.Dashboard.Datapayload;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Alvianda.AI.Dashboard.Services
{
    public interface IWinedatasetService
    {
        Task<List<WinesetEntry>> GetPaginatedResult(string logCategory,int currentPage, int pageSize = 10);
        Task<int> GetCount(string wineCategory);
    }

    public class WinedatasetService : IWinedatasetService
    {
        private HttpClient _httpClient;
        IConfiguration _configuration;

        public WinedatasetService(HttpClient client,
                                    IConfiguration configuration)
        {
            _httpClient = client;
            _configuration = configuration;
        }

        public async Task<List<WinesetEntry>> GetPaginatedResult(string wineCategory,int currentPage, int pageSize = 10)
        {
            var data = await GetWinesetEntries(wineCategory);
            return data.OrderBy(d => d.Id).Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
        }

        public async Task<int> GetCount(string wineCategory)
        {
            var data = await GetWinesetEntries(wineCategory);
            return data.Count;
        }

        private async Task<List<WinesetEntry>> GetWinesetEntries(string wineCategory)
        {
            var serviceEndpoint = $"{_configuration.GetValue<string>("WinesetServiceAPI:BaseURI")}{_configuration.GetValue<string>("WinesetServiceAPI:WinesetRouting")}/entries/{wineCategory}";
            var wineResponse = await _httpClient.GetFromJsonAsync<List<WinesetEntry>>(serviceEndpoint);

            var winesetEntries = new List<WinesetEntry>();
            foreach (var line in wineResponse)
            {
                var record = new WinesetEntry()
                {
                    //Id = logEntry.Id,
                    //InstanceId = logEntry.InstanceId,
                    //Message = logEntry.Message,
                    //MessageShort = logEntry.MessageShort,
                    //Source = logEntry.Source,
                    //TimeGenerated = logEntry.TimeGenerated,
                    //UserName = logEntry.UserName,
                    //MachineName = logEntry.MachineName
                };
                winesetEntries.Add(record);
            }
            return winesetEntries;
        }

        


    }
}
