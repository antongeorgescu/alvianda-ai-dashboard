using System;
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
    }
}
