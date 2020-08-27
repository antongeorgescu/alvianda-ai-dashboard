using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using NUglify;
using NUglify.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestProject
{
    [TestClass]
    public class StoredObjectManagementUnitTests
    {
        [TestMethod]
        public async Task InitializeStorageDbRecords()
        {
            HttpClient _httpClient = new HttpClient();
            var serviceEndpoint = @"http://localhost:53535/api/storagemanagement/test/initializedb";
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
            Assert.IsTrue(jsonDetails.Value<string>().Contains("Successful db initialization."));

        }

        [TestMethod]
        public async Task RunControllerValidation()
        {
            HttpClient _httpClient = new HttpClient();
            var serviceEndpoint = @"http://localhost:53535/api/storagemanagement/validate";
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
            Assert.IsTrue(jsonDetails.Value<string>().Contains("validate storage management controller"));

        }

        [TestMethod]
        public async Task DeleteAllModelFiles()
        {
            HttpClient _httpClient = new HttpClient();
            var serviceEndpoint = @"http://localhost:53535/api/storagemanagement/deletemodels/allfiles";
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
            Assert.IsTrue(jsonDetails.Value<string>().Contains("validate storage management controller"));

        }

        [TestMethod]
        public async Task DeleteOneFile()
        {
            HttpClient _httpClient = new HttpClient();
            var serviceEndpoint = @"http://localhost:53535/api/storagemanagement/deletemodels/onefile&modelid=dt_3c6ae2c0-9c2b-496e-ad7a-6a0a2598dc62";
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
            Assert.IsTrue(jsonDetails.Value<string>().Contains("validate storage management controller"));

        }

        [TestMethod]
        public async Task DeleteAllDbmodels()
        {
            HttpClient _httpClient = new HttpClient();
            var serviceEndpoint = @"http://localhost:53535/api/storagemanagement/deletemodels/alldbrecs";
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
            Assert.IsTrue(jsonDetails.Value<string>().Contains("validate storage management controller"));

        }

        [TestMethod]
        public async Task DeleteOneSession()
        {
            HttpClient _httpClient = new HttpClient();
            var serviceEndpoint = @"http://localhost:53535/api/storagemanagement/deletesession?sessionid=319b44d5-71b6-47e3-bcd3-a94ba27a0f2b";
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

            var jsonResponse = JToken.Parse(responseString);
            Assert.IsTrue(int.Parse(jsonResponse.Value<string>().Split(':')[1]) == 1);

        }

        [TestMethod]
        public async Task ReadAllWorksessions()
        {
            HttpClient _httpClient = new HttpClient();
            var serviceEndpoint = @"http://localhost:53535/api/storagemanagement/worksession/all";
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

            var jsonWsList = JArray.Parse(responseString);
            foreach (var rec in jsonWsList)
            {
                Trace.WriteLine(rec["SessionId"].Value<string>());
            }

            Assert.IsTrue(jsonWsList.Count > 0);

        }

        [TestMethod]
        public async Task ReadApplicationDataBySession()
        {
            HttpClient _httpClient = new HttpClient();
            var serviceEndpoint = @"http://localhost:53535/api/storagemanagement/applicationdata?sessionid=3c6ae2c0-9c2b-496e-ad7a-6a0a2598dc62";
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

            var jsonAppDataList = JArray.Parse(responseString);
            foreach (var rec in jsonAppDataList)
            {
                Trace.WriteLine(rec["DataobjectName"].Value<string>());
            }

            Assert.IsTrue(jsonAppDataList.Count > 0);

        }
    }
}
