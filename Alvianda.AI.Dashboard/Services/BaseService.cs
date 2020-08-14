using NUglify;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Alvianda.AI.Dashboard.Services
{
    public class BaseService
    {
        private HttpClient _httpClient;

        public BaseService(HttpClient client)
        {
            _httpClient = client;
        }

        protected async Task<Tuple<string, string>> HttpGetRequest(string serviceEndpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(serviceEndpoint).ConfigureAwait(true);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                if (responseString.Contains("!DOCTYPE HTML PUBLIC",StringComparison.InvariantCulture))
                {
                    responseString = string.Concat("\"", responseString.Replace('"', '*'), "\"");
                    var result = Uglify.HtmlToText(responseString);
                    var resultCode = result.Code.Replace('"', ' ');
                    throw new Exception(resultCode);
                    //return new Tuple<string, string>("error", resultCode);
                }
                else
                    return new Tuple<string, string>("data", responseString);
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message} [Source={ex.Source}:{ex.StackTrace}]");
                //return new Tuple<string, string>("error", $"{ex.Message} [Source={ex.Source}:{ex.StackTrace}]");
            }
        }

        protected async Task<Tuple<string, string>> HttpPostRequest(string serviceEndpoint, HttpContent content)
        {
            try
            {
                var response = await _httpClient.PostAsync(serviceEndpoint,content).ConfigureAwait(true);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                if (responseString.Contains("!DOCTYPE HTML PUBLIC",StringComparison.CurrentCulture))
                {
                    responseString = string.Concat("\"", responseString.Replace('"', '*'), "\"");
                    var result = Uglify.HtmlToText(responseString);
                    var resultCode = result.Code.Replace('"', ' ');
                    throw new Exception(resultCode);
                    //return new Tuple<string, string>("error", resultCode);
                }
                else
                    return new Tuple<string, string>("data", responseString);
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message} [Source={ex.Source}:{ex.StackTrace}]");
                //return new Tuple<string, string>("error", $"{ex.Message} [Source={ex.Source}:{ex.StackTrace}]");
            }
        }

    }
}
