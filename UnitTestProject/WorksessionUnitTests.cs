using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUglify;
using LoremNET;
using System.Collections;
using System.Collections.Generic;

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
            var serviceEndpoint = @"http://localhost:53535/api/wineanalytics/runanalyzer/trainmodel";
            var resultCode = string.Empty;

            var testContent = new StringContent(JsonConvert.SerializeObject(new
            {
                sessionid = "1b99ed0b-37f3-4e70-9e54-99f9607675a9",
                algorithm = "decision-tree",
                notes = Lorem.Words(35,40),
                description = Lorem.Words(20, 25)
            }), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(serviceEndpoint, testContent).ConfigureAwait(true);

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
        public async Task UpdateTrainModel_DecisionTree()
        {
            HttpClient _httpClient = new HttpClient();

            var serviceEndpoint = @"http://localhost:53535/api/wineanalytics/runanalyzer/trainmodel/update";

            var testContent = new StringContent(JsonConvert.SerializeObject(new
            {
                sessionid = "3c6ae2c0-9c2b-496e-ad7a-6a0a2598dc62",
                modelid = "dt_3c6ae2c0-9c2b-496e-ad7a-6a0a2598dc62",
                modeldescription = Lorem.Words(25, 35)
            }), Encoding.UTF8, "application/json");

            var resultCode = string.Empty;
            var response = await _httpClient.PostAsync(serviceEndpoint, testContent).ConfigureAwait(true);

            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
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
            var serviceEndpoint = @"http://localhost:53535/api/wineanalytics/runanalyzer/trainmodel/saved/listall";
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

            var sessionId = JArray.Parse(responseString)[0]["sessionid"].Value<string>();

            Assert.IsTrue(sessionId == "3c6ae2c0-9c2b-496e-ad7a-6a0a2598dc62");

        }

        [TestMethod]
        public async Task ReadOneSavedModel()
        {
            HttpClient _httpClient = new HttpClient();
            var serviceEndpoint = @"http://localhost:53535/api/wineanalytics/runanalyzer/trainmodel/saved/listone?sessionid=3c6ae2c0-9c2b-496e-ad7a-6a0a2598dc62&modelid=dt_3c6ae2c0-9c2b-496e-ad7a-6a0a2598dc62";
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

            var modelId = JToken.Parse(responseString)["modelid"].Value<string>();

            Assert.IsTrue(modelId == "dt_3c6ae2c0-9c2b-496e-ad7a-6a0a2598dc62");

        }

        [TestMethod]
        public async Task LoadTrainModel_DecisionTree()
        {
            HttpClient _httpClient = new HttpClient();

            var serviceEndpoint = @"http://localhost:53535/api/wineanalytics/runanalyzer/trainmodel/load?sessionid=3c6ae2c0-9c2b-496e-ad7a-6a0a2598dc62&modelid=dt_3c6ae2c0-9c2b-496e-ad7a-6a0a2598dc62";
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
            var jsonObject = jsonDetails.Children();
            var outList = new List<string>();
            foreach (var attribute in jsonObject)
            {
                outList.Add(attribute.First.ToString());
            }

            Trace.Write(responseString);

            Assert.IsTrue(outList.Count == 3);

        }

        [TestMethod]
        public async Task Predict_DecisionTree()
        {
            HttpClient _httpClient = new HttpClient();

            //************************** Predict with saved model *********************************
            var serviceEndpoint = @"http://localhost:53535/api/wineanalytics/runanalyzer/trainmodel/predict";
            var resultCode = string.Empty;
            var testContent = new StringContent(JsonConvert.SerializeObject(new
            {
                modelid = "dt_3c6ae2c0-9c2b-496e-ad7a-6a0a2598dc62",
                sessionid = "3c6ae2c0-9c2b-496e-ad7a-6a0a2598dc62",
                attributes = "fixed acidity,volatile acidity,citric acid,residual sugar,chlorides,free sulfur dioxide,total sulfur dioxide,density,pH,sulphates,alcohol",
                observations = "7.4,0.7,0.0,1.9,0.076,11.0,34.0,0.9978,3.51,0.56,9.4"
            }), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(serviceEndpoint, testContent).ConfigureAwait(true);

            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            if (responseString.Contains("!DOCTYPE HTML PUBLIC"))
            {
                responseString = string.Concat("\"", responseString.Replace('"', '*'), "\"");
                var result = Uglify.HtmlToText(responseString);
                resultCode = result.Code.Replace('"', ' ');
                Assert.IsFalse(resultCode != string.Empty);
            }

            var listDetail = responseString.Split(':');
            Trace.Write($"{listDetail[0]} --> {listDetail[1]}");

            Assert.IsTrue(listDetail.Length == 2);

        }
    }
}
