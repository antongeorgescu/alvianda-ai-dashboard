using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Microsoft.JSInterop;
using Alvianda.AI.Dashboard.Services;
using NUglify;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Alvianda.AI.Dashboard.Pages
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

        public string SelectedAlgorithm { get; set; }
        private List<Tuple<string,string>> messages = new List<Tuple<string,string>>();

        private string attributesHistogramTitle;
        private string attributesHistogramChart;
        private string qualityHistogramTitle;
        private string qualityHistogramChart;
        private string qualityValuesDropped;
        private string correlationChart;
        private string correlationTitle;
        private string correlationAttributes;

        private bool isRunDataAvailable = false;
        private string waitMessage = string.Empty;
        //private string userInput;
        //private string messageInput;

        protected override async Task OnInitializedAsync()
        {
            //Uri _url = new Uri("http://localhost:53535/api/wineanalytics/chathub");

            //hubConnection = new HubConnectionBuilder()
            //    //.WithUrl(NavigationManager.ToAbsoluteUri("/chathub"))
            //    .WithUrl(_url)
            //    .Build();
            await ValidateAnalyticsService();
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

        async Task ValidateAnalyticsService()
        {
            var serviceEndpoint = $"{Config.GetValue<string>("WinesetServiceAPI:BaseURI")}{Config.GetValue<string>("WinesetServiceAPI:AnalyticsRouting")}/validate";
            var response = await Http.GetAsync(serviceEndpoint);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();

            messages.Add(new Tuple<string,string>("info",responseString));
        }

        async Task RunMachineLearningAnalysis()
        {
            string responseString = string.Empty;
            try
            {
                isRunDataAvailable = false;
                waitMessage = "Wait while retrieving your records and analyze the data...";
                var serviceEndpoint = $"{Config.GetValue<string>("WinesetServiceAPI:BaseURI")}{Config.GetValue<string>("WinesetServiceAPI:AnalyticsRouting")}/runanalyzer?algorithm={SelectedAlgorithm}";
                var response = await Http.GetAsync(serviceEndpoint);
                //response.EnsureSuccessStatusCode();

                responseString = await response.Content.ReadAsStringAsync();

                if (responseString.Contains("!DOCTYPE HTML PUBLIC"))
                {
                    responseString = string.Concat("\"",responseString.Replace('"', '*'),"\"");
                    var result = Uglify.HtmlToText(responseString);
                    var resultCode = result.Code.Replace('"', ' ');
                    messages.Add(new Tuple<string,string>("error",resultCode));
                }
                else
                {
                    
                    //responseString = responseString.Replace("'", string.Empty);
                    IList<JToken> responseList = JsonConvert.DeserializeObject(responseString) as IList<JToken>;

                    attributesHistogramTitle = responseList[1].Value<string>().Split(',')[0];
                    qualityHistogramTitle = responseList[1].Value<string>().Split(',')[1];

                    attributesHistogramChart = $"http:////localhost:53535//static//{responseList[0].Value<string>().Split(',')[0]}";
                    qualityHistogramChart = $"http:////localhost:53535//static//{responseList[0].Value<string>().Split(',')[1]}";

                    qualityValuesDropped = responseList[2].Value<string>();

                    correlationChart = $"http:////localhost:53535//static//{responseList[3].Value<string>()}";
                    correlationTitle = responseList[4].Value<string>();

                    correlationAttributes = responseList[5].Value<string>();

                    messages.Add(new Tuple<string,string>("info",responseList[6].Value<string>()));

                    waitMessage = string.Empty;
                    isRunDataAvailable = true;
                }
                    
            }
            catch(Exception ex)
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
