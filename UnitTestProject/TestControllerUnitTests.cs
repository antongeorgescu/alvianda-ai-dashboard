using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using NUglify;
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
    public class TestControllerUnitTests
    {
        [TestMethod]
        public async Task TestSaveModelToFile()
        {
            HttpClient _httpClient = new HttpClient();
            var serviceEndpoint = @"http://localhost:53535/api/testcontroller/trainmodel/save/tofile?sessionid=3c6ae2c0-9c2b-496e-ad7a-6a0a2598dc62";
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

            Trace.WriteLine(responseString);
            Assert.IsTrue(responseString.Contains("Model saved to"));

        }

        [TestMethod]
        public async Task TestLoadModelFromFile()
        {
            HttpClient _httpClient = new HttpClient();
            var serviceEndpoint = @"http://localhost:53535/api/testcontroller/trainmodel/load/fromfile?modelid=dt_3c6ae2c0-9c2b-496e-ad7a-6a0a2598dc62";
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
            Trace.WriteLine(responseString);

            Assert.IsTrue(responseString.Contains("Score:"));

        }

        [TestMethod]
        public async Task TestSaveModelToDatabase()
        {
            HttpClient _httpClient = new HttpClient();
            var serviceEndpoint = @"http://localhost:53535/api/testcontroller/trainmodel/save/todatabase?sessionid=3c6ae2c0-9c2b-496e-ad7a-6a0a2598dc62";
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

            Trace.WriteLine(responseString);
            Assert.IsTrue(responseString.Contains("Model saved into"));

        }

        [TestMethod]
        public async Task TestLoadModelFromDatabase()
        {
            HttpClient _httpClient = new HttpClient();
            var serviceEndpoint = @"http://localhost:53535/api/testcontroller/trainmodel/load/fromdatabase?modelid=dt_3c6ae2c0-9c2b-496e-ad7a-6a0a2598dc62";
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
            Trace.WriteLine(responseString);

            Assert.IsTrue(responseString.Contains("Score:"));

        }
    }
}
