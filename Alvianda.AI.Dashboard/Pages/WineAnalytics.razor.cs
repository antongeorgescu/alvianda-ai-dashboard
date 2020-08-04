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
using System.Json;
using System.Linq;
using Newtonsoft.Json.Linq;

using Microsoft.AspNetCore.SignalR.Client;
using NUglify;

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
                    responseString = responseString.Replace('"', ' ');
                    messages.Add(new Tuple<string,string>("info",responseString));
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
