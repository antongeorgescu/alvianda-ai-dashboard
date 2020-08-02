using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Configuration;
using Alvianda.AI.Dashboard.Datapayload;
using System.Net.Http;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Web;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using System.Linq;
using System.Reflection.PortableExecutable;
using Alvianda.AI.Dashboard.Models.Settings;
using Alvianda.AI.Dashboard.Services;
using Alvianda.AI.Dashboard.Settings;

namespace Alvianda.AI.Dashboard.Pages
{
    [Authorize]
    public partial class WineDataset : ComponentBase
    {
        [Inject] HttpClient Http { get; set; }
        [Inject] IAccessTokenProvider AuthService { get; set; }
        [Inject] IConfiguration Config { get; set; }
        [Inject] WinedatasetService DataService { get; set; }
        [Inject] IJSRuntime jsRuntime { get; set; }
        
        public PaginatedList<WinesetEntry> paginatedList; // = new PaginatedList<EventViewerEntry>();

        public string SelectedWineset { get; set; }
        private string LongMessage { get; set; }
        
        private List<WinesetEntry> WinesetEntries;

        //private IDictionary<long, Tuple<string, string>> DetailBtnAttributes = new Dictionary<long, Tuple<string, string>>();
        CultureInfo provider = CultureInfo.InvariantCulture;
        
        int totalPages { get; set; }
        int pageIndex;
        bool hasNextPage;
        bool hasPreviousPage;
        static int maxRecords;

        // Page and Sort data
        int? pageNumber = 1;
        //string currentSortField = "Name";
        //string currentSortOrder = "Asc";

        int CapMaxRecs;

        private bool isError;
        private string retrievEntriesMsg = "Retrieving records...";

        protected override async Task OnInitializedAsync()
        {
            await GetCappedMaxRecs();

            paginatedList = new PaginatedList<WinesetEntry>();
        }

        public async Task PageIndexChanged(int newPageNumber)
        {
            if (newPageNumber < 1 || newPageNumber > totalPages)
            {
                return;
            }
            pageNumber = newPageNumber;

            LongMessage = null;
            if (WinesetEntries != null)
                WinesetEntries = null;
            await GetWinesetEntries();

            StateHasChanged();
        }

        private async Task GetCappedMaxRecs()
        {
            try
            {
                var serviceEndpoint = $"{Config.GetValue<string>("WinesetServiceAPI:BaseURI")}{Config.GetValue<string>("WinesetServiceAPI:WinesetRouting")}/data/capmaxrecs";
                this.CapMaxRecs = await Http.GetFromJsonAsync<int>(serviceEndpoint);
                maxRecords = this.CapMaxRecs;
            }
            catch (Exception exception)
            {
                LongMessage = $"Error:{exception.Message} [{exception.InnerException.Message}]";
            }
        }

        
        private async Task GetWinesetEntries()
        {
            LongMessage = null;
            
            if (SelectedWineset == "None")
                return;

            try
            {
                // get a new access token if not already cached
                if (Http.DefaultRequestHeaders.Authorization == null)
                {
                    var tokenResult = await AuthService.RequestAccessToken();
                    string jwt = string.Empty;
                    if (tokenResult.TryGetToken(out var token))
                    {
                        Http.DefaultRequestHeaders.Add("Authorization", $"Bearer {token.Value}");
                        jwt = $"Bearer {token.Value}";
                    }
                }

                //var serviceEndpoint = $"{Config.GetSection("EventViewerLoggerAPI").GetValue<string>("BaseURI")}/api/logreader/logs/records/{SelectedLog}/{pageNumber}";
                var serviceEndpoint = $"{Config.GetValue<string>("WinesetServiceAPI:BaseURI")}{Config.GetValue<string>("WinesetServiceAPI:WinesetRouting")}/entries/{SelectedWineset}/{pageNumber}";
                var response = await Http.GetAsync(serviceEndpoint);
                response.EnsureSuccessStatusCode();

                using var responseStream = await response.Content.ReadAsStreamAsync();
                var paginatedList = await System.Text.Json.JsonSerializer.DeserializeAsync<PaginatedList<WinesetEntry>>(responseStream, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true,
                });

                WinesetEntries = paginatedList.Items;

                //DetailBtnAttributes.Clear();
                //LogEntries.ForEach(x => DetailBtnAttributes.Add(x.Id, new Tuple<string, string>("btn btn-success", "Show Details")));

                totalPages = paginatedList.TotalPages;
                pageIndex = paginatedList.PageIndex;
                hasNextPage = paginatedList.HasNextPage;
                hasPreviousPage = paginatedList.HasPreviousPage;
                maxRecords = paginatedList.MaxRecords;

                StateHasChanged();

            }
            catch (AccessTokenNotAvailableException exception)
            {
                isError = true;
                retrievEntriesMsg = string.Empty;
                LongMessage = $"Authorization error:{exception.Message} [{exception.InnerException.Message}]";
            }
            catch (UnauthorizedAccessException exception)
            {
                isError = true;
                retrievEntriesMsg = string.Empty;
                LongMessage = $"Authorization error:{exception.Message} [{exception.InnerException.Message}]";
            }
            catch (Exception exception)
            {
                isError = true;
                //LongMessage = $"Error:{exception.Message} [{exception.InnerException.Message}]";
                retrievEntriesMsg = string.Empty;
                LongMessage = $"Error:{exception.Message}";
            }
        }

        private void ChangedCappedMaxRecs(ChangeEventArgs e)
        {
            this.CapMaxRecs = int.Parse(e.Value.ToString());
        }
        private async void UpdateCappedMaxRecs()
        {
            
            var serviceEndpoint = $"{Config.GetValue<string>("WinesetServiceAPI:BaseURI")}{Config.GetValue<string>("WinesetServiceAPI:WinesetRouting")}/entries/capmaxrecs";

            //var payload = "{\"CapSize\":\"" + CapMaxRecs.ToString() + "\"}";
            CappedRecsSettings payload = new CappedRecsSettings() { CappedMaxRecs = this.CapMaxRecs.ToString() };
            string jsonpayload = await Task.Run(() => JsonConvert.SerializeObject(payload));
            HttpContent c = new StringContent(jsonpayload, Encoding.UTF8, "application/json");
            
            var response = await Http.PostAsync(serviceEndpoint,c);
            response.EnsureSuccessStatusCode();

            maxRecords = this.CapMaxRecs;

        }

        private void ClearEntryDetails()
        {
            LongMessage = null;
        }
    }
}
