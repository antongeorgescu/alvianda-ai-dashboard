using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using NUglify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestProject
{
    [TestClass]
    public class StoredObjectManagement
    {
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
    }
}
