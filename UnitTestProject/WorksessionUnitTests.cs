using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using NUglify;

namespace UnitTestProject
{
    [TestClass]
    public class WorksessionUnitTests
    {
        [TestMethod]
        public async Task ReturnSessionDetails()
        {
            HttpClient _httpClient = new HttpClient();
            var serviceEndpoint = @"http://localhost:53535/api/wineanalytics/worksessions/details?sessionid=85ce9e27-b727-4a95-b218-1a4a9e0bb6a9";
            var resultCode = string.Empty;
            var response = await _httpClient.GetAsync(serviceEndpoint).ConfigureAwait(true);
            
            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            if (responseString.Contains("!DOCTYPE HTML PUBLIC"))
            {
                responseString = string.Concat("\"", responseString.Replace('"', '*'), "\"");
                var result = Uglify.HtmlToText(responseString);
                resultCode = result.Code.Replace('"', ' ');
                Assert.IsFalse(resultCode != string.Empty);
            }
            
            var jsonDetails = JToken.Parse(responseString);
            Assert.IsTrue(jsonDetails[0]["Description"].Value<string>().Contains("test 1122"));

        }

        [TestMethod]
        public async Task RunTrainModel()
        {
            HttpClient _httpClient = new HttpClient();
            var serviceEndpoint = @"http://localhost:53535/api/wineanalytics/runanalyzer/trainmodel?algorithm=decision-tree&sessionid=85ce9e27-b727-4a95-b218-1a4a9e0bb6a9";
            var resultCode = string.Empty;
            var response = await _httpClient.GetAsync(serviceEndpoint).ConfigureAwait(true);
            
            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            if (responseString.Contains("!DOCTYPE HTML PUBLIC"))
            {
                responseString = string.Concat("\"", responseString.Replace('"', '*'), "\"");
                var result = Uglify.HtmlToText(responseString);
                resultCode = result.Code.Replace('"', ' ');
                Assert.IsFalse(resultCode != string.Empty);
            }

            var jsonDetails = JToken.Parse(responseString);
            Assert.IsTrue(jsonDetails[0]["Description"].Value<string>().Contains("test 1122"));

        }

        [TestMethod]
        public async Task GetSavedDataFrameFromJson()
        {
            HttpClient _httpClient = new HttpClient();
            var serviceEndpoint = @"http://localhost:53535/api/wineanalytics/runanalyzer/persist/json/dataframe?sessionid=85ce9e27-b727-4a95-b218-1a4a9e0bb6a9";
            var resultCode = string.Empty;
            var response = await _httpClient.GetAsync(serviceEndpoint).ConfigureAwait(true);

            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            if (responseString.Contains("!DOCTYPE HTML PUBLIC"))
            {
                responseString = string.Concat("\"", responseString.Replace('"', '*'), "\"");
                var result = Uglify.HtmlToText(responseString);
                resultCode = result.Code.Replace('"', ' ');
                Assert.IsFalse(resultCode != string.Empty);
            }

            var jsonDetails = JToken.Parse(responseString);
            Assert.IsTrue(jsonDetails[0]["Description"].Value<string>().Contains("test 1122"));

        }

        [TestMethod]
        public async Task GetSavedDataFrameFromDb()
        {
            HttpClient _httpClient = new HttpClient();
            var serviceEndpoint = @"http://localhost:53535/api/wineanalytics/runanalyzer/persist/db/dataframe?sessionid=3c6ae2c0-9c2b-496e-ad7a-6a0a2598dc62";
            var resultCode = string.Empty;
            var response = await _httpClient.GetAsync(serviceEndpoint).ConfigureAwait(true);

            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            if (responseString.Contains("!DOCTYPE HTML PUBLIC"))
            {
                responseString = string.Concat("\"", responseString.Replace('"', '*'), "\"");
                var result = Uglify.HtmlToText(responseString);
                resultCode = result.Code.Replace('"', ' ');
                Assert.IsFalse(resultCode != string.Empty);
            }
            Assert.IsTrue(responseString.Contains("3c6ae2c0-9c2b-496e-ad7a-6a0a2598dc62"));

        }

        [TestMethod]
        public async Task TrainModel_DecisionTree()
        {
            HttpClient _httpClient = new HttpClient();
            var serviceEndpoint = @"http://localhost:53535/api/wineanalytics/runanalyzer/trainmodel?sessionid=3c6ae2c0-9c2b-496e-ad7a-6a0a2598dc62&algorithm=decision-tree";
            var resultCode = string.Empty;
            var response = await _httpClient.GetAsync(serviceEndpoint).ConfigureAwait(true);

            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            if (responseString.Contains("!DOCTYPE HTML PUBLIC"))
            {
                responseString = string.Concat("\"", responseString.Replace('"', '*'), "\"");
                var result = Uglify.HtmlToText(responseString);
                resultCode = result.Code.Replace('"', ' ');
                Assert.IsFalse(resultCode != string.Empty);
            }

            var jsonDetails = JToken.Parse(responseString);

            Trace.Write(jsonDetails.Value<string>());

            Assert.IsTrue(jsonDetails.Value<string>().Split('|').Length == 3);

        }

        [TestMethod]
        public async Task AboutConfusionMatrix()
        {
            HttpClient _httpClient = new HttpClient();
            var serviceEndpoint = @"http://localhost:53535/api/wineanalytics/runanalyzer/about_confusion_matrix";
            var resultCode = string.Empty;
            var response = await _httpClient.GetAsync(serviceEndpoint).ConfigureAwait(true);

            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            if (responseString.Contains("!DOCTYPE HTML PUBLIC"))
            {
                responseString = string.Concat("\"", responseString.Replace('"', '*'), "\"");
                var result = Uglify.HtmlToText(responseString);
                resultCode = result.Code.Replace('"', ' ');
                Assert.IsFalse(resultCode != string.Empty);
            }

            Console.Write(responseString);
            Assert.IsTrue(responseString.Length > 100);

        }
    }
}
