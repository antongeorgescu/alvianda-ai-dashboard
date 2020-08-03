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
using System.Globalization;
using Alvianda.AI.Dashboard.Services;

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
        
        // Page and Sort data
        int? pageNumber = 1;
        //string currentSortField = "Name";
        //string currentSortOrder = "Asc";

        private bool isError;
        private string retrievEntriesMsg = "Retrieving records...";

        protected override async Task OnInitializedAsync()
        {
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

        private async Task GetWinesetEntries()
        {
            LongMessage = null;
            
            if (SelectedWineset == "None")
                return;

            try
            {
                // get a new access token if not already cached
                //if (Http.DefaultRequestHeaders.Authorization == null)
                //{
                //    var tokenResult = await AuthService.RequestAccessToken();
                //    string jwt = string.Empty;
                //    if (tokenResult.TryGetToken(out var token))
                //    {
                //        Http.DefaultRequestHeaders.Add("Authorization", $"Bearer {token.Value}");
                //        jwt = $"Bearer {token.Value}";
                //    }
                //}

                var serviceEndpoint = $"{Config.GetValue<string>("WinesetServiceAPI:BaseURI")}{Config.GetValue<string>("WinesetServiceAPI:WinesetRouting")}/entries/{SelectedWineset}?pageno={pageNumber}";
                var response = await Http.GetAsync(serviceEndpoint);
                response.EnsureSuccessStatusCode();

                using var responseStream = await response.Content.ReadAsStreamAsync();
                var paginatedList = await System.Text.Json.JsonSerializer.DeserializeAsync<PaginatedList<WinesetEntry>>(responseStream, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true,
                });

                WinesetEntries = paginatedList.Items;

                totalPages = paginatedList.TotalPages;
                pageIndex = paginatedList.PageIndex;
                hasNextPage = paginatedList.HasNextPage;
                hasPreviousPage = paginatedList.HasPreviousPage;
                
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
    }
}
