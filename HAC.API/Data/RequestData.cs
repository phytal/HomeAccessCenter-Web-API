using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sentry;

namespace HAC.API.Data {
    public static class RequestData {
        public static async Task<string> GetData(HttpClient httpClient, string link, ResponseType type,
            string section = "Student", string param = "") {
            var requestLink = $"{link}/HomeAccess/Content/{section}/{type}.aspx{param}";
            try {
                foreach (var (key, value) in Login.HandlerProperties) httpClient.DefaultRequestHeaders.Add(key, value);

                // tries to post a request with the http client
                try {
                    var response = await httpClient.GetStringAsync(requestLink);

                    return response;
                }
                catch (HttpRequestException e) {
                    SentrySdk.CaptureException(e);
                    return null;
                }
            }
            catch {
                return null;
            }
        }

        public static async Task<string> GetDataWithBody(HttpClient httpClient, string link, ResponseType type,
            string body, string section = "Student") {
            try {
                var reqLink = $"{link}/HomeAccess/Content/{section}/{type}.aspx";
                httpClient.DefaultRequestHeaders.Referrer =
                    new Uri(reqLink);
                httpClient.DefaultRequestHeaders.CacheControl = CacheControlHeaderValue.Parse("max-age=0");
                httpClient.DefaultRequestHeaders.ExpectContinue = false;
                httpClient.DefaultRequestHeaders.Add("Origin", @$"{link}/");
                foreach (var (key, value) in Login.HandlerProperties) httpClient.DefaultRequestHeaders.Add(key, value);

                var data = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");

                // tries to post a request with the http client
                try {
                    var response = await httpClient.PostAsync(reqLink, data);

                    response.EnsureSuccessStatusCode();

                    var responseBody = await response.Content.ReadAsStringAsync();

                    return responseBody;
                }
                catch (HttpRequestException e) {
                    SentrySdk.CaptureException(e);
                    return null;
                }
            }
            catch {
                return null;
            }
        }
    }

    public enum ResponseType {
        Transcript,
        ReportCards,
        Assignments,
        InterimProgress,
        Registration,
        ClassPopUp,
        MonthlyView
    }
}