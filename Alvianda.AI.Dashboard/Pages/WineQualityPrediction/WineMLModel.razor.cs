using Alvianda.AI.Dashboard.ServiceModels;
using Alvianda.AI.Dashboard.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUglify;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

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
        public string SelectedAlgorithmType { get; set; }
        IList<Algorithm> Algorithms;

        public string SelectedSessionId { get; set; }
        IList<WorkingSession> WorkingSessions;
        
        private List<Tuple<string, string>> messages = new List<Tuple<string, string>>();

        private bool isModelDataAvailable = false;
        private bool isAlgorithmListAvailable = false;
        private bool isSessionsListAvailable = false;
        private bool isSavedDODetailsAvailable = false;

        private string PersistedDODetails;

        private string waitMessage = string.Empty;

        WineMlmodelService mlmodelService;

        protected override async Task OnInitializedAsync()
        {
            mlmodelService = new WineMlmodelService(Http, Config);
        }

        async Task PopulateAlgorithmList(ChangeEventArgs arg)
        {
            string algorithmType = arg.Value.ToString();
            await GetAlgorithmList(algorithmType).ConfigureAwait(true);

            await PopulateWorkingSessionList(1).ConfigureAwait(true);
        }

        async Task PopulateSessionDetails(ChangeEventArgs arg)
        {
            SelectedSessionId = arg.Value.ToString();

            await GetSessionDetails(SelectedSessionId).ConfigureAwait(true);

        }

        async Task GetAlgorithmList(string algorithmType)
        {
            try
            {
                isAlgorithmListAvailable = false;
                var response = await mlmodelService.GetAlgorithmList(algorithmType).ConfigureAwait(true);
                if (response.Item1 == "info")
                {
                    Algorithms = response.Item2;
                    isAlgorithmListAvailable = true;
                    return;
                }
                if (response.Item1 == "error")
                {
                    throw new Exception(response.Item3);
                }
            }
            catch(Exception ex)
            {
                messages.Add(new Tuple<string, string>("error", ex.Message));
            }
        }

        async Task GetSessionDetails(string sessionId)
        {
            try
            {
                this.isSavedDODetailsAvailable = false;
                var response = await mlmodelService.GetSessionDetails(sessionId).ConfigureAwait(true);
                if (response.Item1 == "info")
                {
                    var jsonData = response.Item2 as JToken;
                    PersistedDODetails = $"Description:{jsonData["Description"]}{Environment.NewLine}" + 
                        $"Notes:{jsonData["Notes"]}{Environment.NewLine}" +
                        $"Created On:{jsonData["CreatedOn"]}";
                    isSavedDODetailsAvailable = true;
                    return;
                }
                if (response.Item1 == "error")
                {
                    throw new Exception(response.Item3);
                }
            }
            catch (Exception ex)
            {
                messages.Add(new Tuple<string, string>("error", ex.Message));
            }
        }


        async Task PopulateWorkingSessionList(int applicationId)
        {
            try
            {
                isSessionsListAvailable = false;
                var response = await mlmodelService.GetWorkingSessionList(applicationId).ConfigureAwait(true);
                if (response.Item1 == "info")
                {
                    WorkingSessions = response.Item2;
                    isSessionsListAvailable = true;
                    return;
                }
                if (response.Item1 == "error")
                {
                    throw new Exception(response.Item3);
                }
            }
            catch (Exception ex)
            {
                messages.Add(new Tuple<string, string>("error", ex.Message));
            }
        }

        async Task RunMachineLearningModel()
        {
            string responseString = string.Empty;
            try
            {
                isModelDataAvailable = false;
                waitMessage = "Wait while we are running the selected algorithm and analyze the model...";

                var responseDictionary = await mlmodelService.RunMachineLearningModel(SelectedAlgorithm).ConfigureAwait(true);
                if (responseDictionary.ContainsKey("error"))
                {
                    messages.Add(new Tuple<string, string>("error", responseDictionary["error"]));
                }
                else
                {
                    //attributesHistogramTitle = responseDictionary["attributesHistogramTitle"];
                    //qualityHistogramTitle = responseDictionary["qualityHistogramTitle"];

                    //attributesHistogramChart = responseDictionary["attributesHistogramChart"];
                    //qualityHistogramChart = responseDictionary["qualityHistogramChart"];

                    //qualityValuesDropped = responseDictionary["qualityValuesDropped"];

                    //correlationChart = responseDictionary["correlationChart"];
                    //correlationTitle = responseDictionary["correlationTitle"];

                    //correlationAttributes = responseDictionary["correlationAttributes"];

                    messages.Add(new Tuple<string, string>("info", responseDictionary["infomessage"]));

                    waitMessage = string.Empty;
                    isModelDataAvailable = true;
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
