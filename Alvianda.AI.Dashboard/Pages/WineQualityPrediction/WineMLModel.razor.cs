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

namespace Alvianda.AI.Dashboard.Pages.WineQualityPrediction
{
    public partial class WineMLModel : ComponentBase
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
                
        private bool isRunDataAvailable = false;
        private string waitMessage = string.Empty;

        private string resultOneName;
        private string resultOneData;

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
            try
            {
                var serviceEndpoint = $"{Config.GetValue<string>("WinesetServiceAPI:BaseURI")}{Config.GetValue<string>("WinesetServiceAPI:AnalyticsRouting")}/validate";
                var response = await Http.GetAsync(serviceEndpoint);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                if (responseString.Contains("!DOCTYPE HTML PUBLIC"))
                {
                    responseString = string.Concat("\"", responseString.Replace('"', '*'), "\"");
                    var result = Uglify.HtmlToText(responseString);
                    var resultCode = result.Code.Replace('"', ' ');
                    messages.Add(new Tuple<string, string>("error", resultCode));
                }
                else
                    messages.Add(new Tuple<string, string>("info", responseString));
            }
            catch(Exception ex)
            {
                messages.Add(new Tuple<string, string>("error", $"{ex.Message} [Source={ex.Source}:{ex.StackTrace}]"));
            }
        }

        async Task RunMachineLearningModel()
        {
            string responseString = string.Empty;
            try
            {
                isRunDataAvailable = false;
                waitMessage = "Wait while retrieving machine learning model generation data...";
                var serviceEndpoint = $"{Config.GetValue<string>("WinesetServiceAPI:BaseURI")}{Config.GetValue<string>("WinesetServiceAPI:AnalyticsRouting")}/runanalyzer/dataset";
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

                    messages.Add(new Tuple<string,string>("info","Received modeling data from server..."));

                    resultOneName = "Result-One Name";
                    resultOneData = "Result-One Data";

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
