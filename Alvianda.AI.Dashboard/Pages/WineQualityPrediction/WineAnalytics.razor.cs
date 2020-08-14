using Alvianda.AI.Dashboard.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Alvianda.AI.Dashboard.Pages.WineQualityPrediction
{
    public partial class WineAnalytics : ComponentBase
    {
        [Inject] HttpClient Http { get; set; }
        [Inject] IAccessTokenProvider AuthService { get; set; }
        [Inject] IConfiguration Config { get; set; }
        [Inject] WinedatasetService DataService { get; set; }
        [Inject] IJSRuntime jsRuntime { get; set; }
        //[Inject] NavigationManager NavigationManager { get; set; }

        //private HubConnection hubConnection;

        //public string SelectedAlgorithm { get; set; }
        private List<Tuple<string, string>> messages = new List<Tuple<string, string>>();

        private string attributesHistogramTitle;
        private string attributesHistogramChart;
        private string qualityHistogramTitle;
        private string qualityHistogramChart;
        private string qualityValuesDropped;
        private string correlationChart;
        private string correlationTitle;
        private string correlationAttributes;

        // Visibility attributes
        bool isVisibleChartHistogram = false;
        bool isVisibleQualityHistogram = false;
        bool isVisibleCorrelationAttributes = false;
        bool isVisibleQualityValuesDropped = false;
        bool isVisibleCorrelationChart = false;

        private int chartWidth;
        private int chartHeight;

        private bool isRunDatasetAnalysisAvailable = false;
        private string waitMessage = string.Empty;
        //private string userInput;
        //private string messageInput;

        WinePreparedataService prepdataService;

        protected override async Task OnInitializedAsync()
        {
            //Uri _url = new Uri("http://localhost:53535/api/wineanalytics/chathub");

            //hubConnection = new HubConnectionBuilder()
            //    //.WithUrl(NavigationManager.ToAbsoluteUri("/chathub"))
            //    .WithUrl(_url)
            //    .Build();

            prepdataService = new WinePreparedataService(Http, Config);
            chartWidth = Config.GetValue<int>("AppSettings:ModalDialog:Width");
            chartHeight = Config.GetValue<int>("AppSettings:ModalDialog:Height");
        }

        async Task TestDataPreparationService()
        {
            await ValidateGetAnalyticsService().ConfigureAwait(true);
            await ValidatePostAnalyticsService().ConfigureAwait(true);
        }

        //async Task Connect()
        //{
        //    hubConnection.On<string, string>("ReceiveMessage", (user, message) =>
        //    {
        //        var encodedMsg = $"{user}: {message}";
        //        messages.Add(encodedMsg);
        //        StateHasChanged();
        //    });

        //    //await hubConnection.StartAsync();
        //}

        async Task ValidateGetAnalyticsService()
        {
            Tuple<string, string> response = await prepdataService.ValidateGetPrepDataService().ConfigureAwait(true);
            if (response.Item1 == "data")
                messages.Add(new Tuple<string, string>("info", response.Item2));
        }

        async Task ValidatePostAnalyticsService()
        {
            Tuple<string, string> response = await prepdataService.ValidatePostPrepDataService().ConfigureAwait(true);
            if (response.Item1 == "data")
                messages.Add(new Tuple<string, string>("info", response.Item2));
        }

        async Task RunDatasetAnalysis()
        {
            string responseString = string.Empty;
            try
            {
                isRunDatasetAnalysisAvailable = false;
                waitMessage = "Wait while retrieving dataset records and analyze the data...";

                var responseDictionary = await prepdataService.RunPrepDataAnalysis();
                if (responseDictionary.ContainsKey("error"))
                {
                    messages.Add(new Tuple<string, string>("error", responseDictionary["error"]));
                }
                else
                {
                    attributesHistogramTitle = responseDictionary["attributesHistogramTitle"];
                    qualityHistogramTitle = responseDictionary["qualityHistogramTitle"];

                    attributesHistogramChart = responseDictionary["attributesHistogramChart"];
                    qualityHistogramChart = responseDictionary["qualityHistogramChart"];

                    qualityValuesDropped = responseDictionary["qualityValuesDropped"];

                    correlationChart = responseDictionary["correlationChart"];
                    correlationTitle = responseDictionary["correlationTitle"];

                    correlationAttributes = responseDictionary["correlationAttributes"];

                    messages.Add(new Tuple<string, string>("info", responseDictionary["infomessage"]));

                    waitMessage = string.Empty;
                    isRunDatasetAnalysisAvailable = true;
                }

            }
            catch (Exception ex)
            {
                messages.Add(new Tuple<string, string>("error", ex.Message));
            }
        }

        async Task PersistProcessedData()
        {
            string responseString = string.Empty;
            try
            {
                isRunDatasetAnalysisAvailable = false;
                waitMessage = "Wait while retrieving dataset records and analyze the data...";

                var responseDictionary = await prepdataService.RunPrepDataAnalysis();
                if (responseDictionary.ContainsKey("error"))
                {
                    messages.Add(new Tuple<string, string>("error", responseDictionary["error"]));
                }
                else
                {
                    attributesHistogramTitle = responseDictionary["attributesHistogramTitle"];
                    qualityHistogramTitle = responseDictionary["qualityHistogramTitle"];

                    attributesHistogramChart = responseDictionary["attributesHistogramChart"];
                    qualityHistogramChart = responseDictionary["qualityHistogramChart"];

                    qualityValuesDropped = responseDictionary["qualityValuesDropped"];

                    correlationChart = responseDictionary["correlationChart"];
                    correlationTitle = responseDictionary["correlationTitle"];

                    correlationAttributes = responseDictionary["correlationAttributes"];

                    messages.Add(new Tuple<string, string>("info", responseDictionary["infomessage"]));

                    waitMessage = string.Empty;
                    isRunDatasetAnalysisAvailable = true;
                }

            }
            catch (Exception ex)
            {
                messages.Add(new Tuple<string, string>("error", ex.Message));
            }
        }

        //Task Send() =>
        //    hubConnection.SendAsync("SendMessage", userInput, messageInput);

        //public bool IsConnected =>
        //    hubConnection.State == HubConnectionState.Connected;

        //public void Dispose()
        //{
        //    _ = hubConnection.DisposeAsync();
        //}
    }
}
