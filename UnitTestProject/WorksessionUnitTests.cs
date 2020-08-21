﻿using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
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
            Assert.IsTrue(jsonDetails["Description"].Value<string>().Contains("test 1122"));

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
        public async Task SaveTrainModel_DecisionTree()
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

            var modelId = jsonDetails.Value<string>().Split('|')[2].Split(':')[1].ToString();

            serviceEndpoint = @"http://localhost:53535/api/wineanalytics/runanalyzer/trainmodel/save";

            //var serviceEndpoint = $"{_configuration.GetValue<string>("WinesetServiceAPI:BaseURI")}{_configuration.GetValue<string>("WinesetServiceAPI:AnalyticsRouting")}/validate";

            var testContent = new StringContent(JsonConvert.SerializeObject(new
            {
                sessionid = "3c6ae2c0-9c2b-496e-ad7a-6a0a2598dc62",
                modelid = modelId,
                modeldescription = "Quisque ac commodo orci, et faucibus massa. Praesent elit libero, feugiat vel pellentesque et, tristique ac ante."
            }), Encoding.UTF8, "application/json"); ;

            resultCode = string.Empty;
            response = await _httpClient.PostAsync(serviceEndpoint, testContent).ConfigureAwait(true);

            responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            if (responseString.Contains("!DOCTYPE HTML PUBLIC"))
            {
                responseString = string.Concat("\"", responseString.Replace('"', '*'), "\"");
                var result = Uglify.HtmlToText(responseString);
                resultCode = result.Code.Replace('"', ' ');
                Trace.Write(result);
                Assert.IsFalse(resultCode != string.Empty);
            }

            Trace.Write(responseString);

            Assert.IsTrue(responseString.Length > 30);

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

        [TestMethod]
        public async Task ReadAllSavedModels()
        {
            HttpClient _httpClient = new HttpClient();
            var serviceEndpoint = @"http://localhost:53535/api/wineanalytics/runanalyzer/trainmodel/load/all";
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

            var resultList = JToken.Parse(responseString);

            Assert.IsNotNull(resultList.Children().GetEnumerator());

        }
    }
}
