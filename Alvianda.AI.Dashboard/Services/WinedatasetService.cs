﻿using Alvianda.AI.Dashboard.Datapayload;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Alvianda.AI.Dashboard.Services
{
    public interface IWinedatasetService
    {
        Task<List<WinesetEntry>> GetPaginatedResult(string logCategory, int currentPage, int pageSize = 10);
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

        public async Task<List<WinesetEntry>> GetPaginatedResult(string wineCategory, int currentPage, int pageSize = 10)
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
            var serviceEndpoint = $"{_configuration.GetValue<string>("WinesetServiceAPI:BaseURI")}{_configuration.GetValue<string>("WinesetServiceAPI:DatasetRouting")}/entries/{wineCategory}";
            var wineResponse = await _httpClient.GetFromJsonAsync<List<WinesetEntry>>(serviceEndpoint);

            var winesetEntries = new List<WinesetEntry>();
            foreach (var line in wineResponse)
            {
                var record = new WinesetEntry()
                {
                    Id = line.Id,
                    FixedAcidity = line.FixedAcidity,
                    VolatileAcidity = line.VolatileAcidity,
                    CitricAcid = line.CitricAcid,
                    Chlorides = line.Chlorides,
                    FreeSulphurDioxide = line.FreeSulphurDioxide,
                    TotalSulphurDioxide = line.TotalSulphurDioxide,
                    Density = line.Density,
                    PH = line.PH,
                    Sulphates = line.Sulphates,
                    Alcohol = line.Alcohol,
                    Quality = line.Quality
                };
                winesetEntries.Add(record);
            }
            return winesetEntries;
        }
    }
}
