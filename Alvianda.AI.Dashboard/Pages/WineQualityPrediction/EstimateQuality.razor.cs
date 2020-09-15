using Alvianda.AI.Dashboard.ServiceModels;
using Alvianda.AI.Dashboard.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUglify;
using NUglify.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Alvianda.AI.Dashboard.Pages.WineQualityPrediction
{
    public partial class EstimateQuality : ComponentBase
    {
        [Inject] HttpClient Http { get; set; }
        [Inject] IAccessTokenProvider AuthService { get; set; }
        [Inject] IConfiguration Config { get; set; }
        [Inject] WinedatasetService DataService { get; set; }
        [Inject] IJSRuntime jsRuntime { get; set; }
        //[Inject] NavigationManager NavigationManager { get; set; }

        //private HubConnection hubConnection;

        //public string SelectedAlgorithm { get; set; }
        //public string SelectedAlgorithmType { get; set; }
        //IList<Algorithm> Algorithms;

        public string SelectedSessionId { get; set; }
        IList<TrainModelSession> TrainModelSessions;
        
        private List<Tuple<string, string>> messages = new List<Tuple<string, string>>();

        private bool isModelDataAvailable = false;
        //private bool isAlgorithmListAvailable = false;
        private bool isSessionsListAvailable = false;
        private bool isSavedDODetailsAvailable = false;

        private string PersistedDODetails;

        private string waitMessage = string.Empty;
        private string description = string.Empty;
        private string notes = string.Empty;

        private List<string> attributeKeys;
        private List<string> attributeNames;
        private List<double> attributeVals;

        private Dictionary<string, string> trainingModelResults;

        private string ResultMessage;
        private string ResultValue;

        WineMlmodelService mlmodelService;

        protected override async Task OnInitializedAsync()
        {
            mlmodelService = new WineMlmodelService(Http, Config);
            await PopulateTrainModelSessionList(1).ConfigureAwait(true);
        }

        async Task PopulateSessionDetails(ChangeEventArgs arg)
        {
            SelectedSessionId = arg.Value.ToString();

            await GetTrainSessionDetails(SelectedSessionId).ConfigureAwait(true);

        }

        async Task GetTrainSessionDetails(string sessionId)
        {
            try
            {
                this.isSavedDODetailsAvailable = false;
                var response = await mlmodelService.GetTrainSessionDetails(sessionId).ConfigureAwait(true);
                if (response.Item1 == "info")
                {
                    var jsonData = response.Item2 as JToken;
                    
                    description = jsonData["Description"].Value<string>();
                    notes = jsonData["Notes"].Value<string>();
                    var algorithmName = jsonData["DisplayName"].Value<string>();
                    var createdOn = jsonData["CreatedOn"].Value<string>();
                    PersistedDODetails = $"Algorithm Name:{algorithmName}{Environment.NewLine}" +  
                        $"Description:{description}{Environment.NewLine}" +
                        $"Notes:{notes}{Environment.NewLine}" +
                        $"Created On:{createdOn}";
                    isSavedDODetailsAvailable = true;

                    attributeKeys = jsonData["DataobjectAttributes"].Value<string>().Split(',').ToList();
                    attributeNames = new List<string>();
                    if (attributeVals == null)
                        attributeVals = new List<double>();
                    foreach (var key in attributeKeys)
                    {
                        //attributeNames.Add(Regex.Replace(key, @"(^\w)|(\s\w)", m => m.Value.ToUpper()));
                        attributeNames.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(key));
                        attributeVals.Add(0.0);
                    }

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


        async Task PopulateTrainModelSessionList(int applicationId)
        {
            try
            {
                isSessionsListAvailable = false;
                var response = await mlmodelService.GetTrainModelSessionList(applicationId).ConfigureAwait(true);
                if (response.Item1 == "info")
                {
                    TrainModelSessions = response.Item2;
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

        async Task RunPrediction()
        {
            string responseString = string.Empty;
            try
            {
                isModelDataAvailable = false;
                waitMessage = "Wait while we are running the selected algorithm and analyze the model...";
                var modelId = $"dt_{SelectedSessionId}";
                var observations = string.Join(",", attributeVals);
                var response = await mlmodelService.RunModelPrediction(modelId,
                                                                  SelectedSessionId,
                                                                  string.Join(",", attributeKeys),
                                                                  observations).ConfigureAwait(true);
                if (response.Item1 == "data")
                {
                    messages.Add(new Tuple<string, string>("info", $"{response.Item2.Split(':')[0]} = {response.Item2.Split(':')[1]}"));
                    ResultMessage = response.Item2.Split(':')[0];
                    ResultValue = response.Item2.Split(':')[1];
                }

                //trainingModelResults = await mlmodelService.RunMachineLearningModel(SelectedAlgorithm, SelectedSessionId, description,notes).ConfigureAwait(true);
                //if (trainingModelResults.ContainsKey("error"))
                //{
                //    messages.Add(new Tuple<string, string>("error", trainingModelResults["error"]));
                //}
                //else
                //{
                //    messages.Add(new Tuple<string, string>("info",$"Trained model finished. {trainingModelResults.Count} results provided (see below)."));

                //    waitMessage = string.Empty;
                //    isModelDataAvailable = true;
                //}
            }
            catch (Exception ex)
            {
                messages.Add(new Tuple<string, string>("error", ex.Message));
            }
        }

        void FillTestObservation()
        {
            attributeVals = new List<double>();
            var listStrAttribs = "7.4,0.7,0.0,1.9,0.076,11.0,34.0,0.9978,3.51,0.56,9.4".Split(",");
            listStrAttribs.ForEach(x => attributeVals.Add(double.Parse(x)));
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
