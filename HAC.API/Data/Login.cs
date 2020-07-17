using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Sentry;

namespace HAC.API.Data {
    public interface ILogin {
        Task<string> LoginAsync(string link, string username, string password);
    }

    public class Login : ILogin {
        // contains the headers needed for a http request
        public static readonly Dictionary<string, string> HandlerProperties = new Dictionary<string, string> {
            {"Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8"},
            {"Accept-Language", "en-US,en;q=0.8"},
            {"Accept-Encoding", "gzip, deflate"},
            {"Connection", "keep-alive"},
            {"DNT", "1"},
            {"Upgrade-Insecure-Requests", "1"}, {
                "User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.89 Safari/537.36"
            }
        };

        private readonly HttpClient _httpClient;

        public Login(HttpClient httpClient) {
            _httpClient = httpClient;
        }

        public async Task<string> LoginAsync(string link, string username, string password) {
            var loginLink = $"{link}/HomeAccess/Account/LogOn";
            try {
                _httpClient.DefaultRequestHeaders.Referrer =
                    new Uri($"{link}/HomeAccess/Account/LogOn");
                _httpClient.DefaultRequestHeaders.CacheControl = CacheControlHeaderValue.Parse("max-age=0");
                _httpClient.DefaultRequestHeaders.ExpectContinue = false;
                _httpClient.DefaultRequestHeaders.Add("Origin", @$"{link}/");
                foreach (var (key, value) in HandlerProperties) _httpClient.DefaultRequestHeaders.Add(key, value);

                var body = @"Database=10&LogOnDetails.UserName=" + username + "&LogOnDetails.Password=" + password;
                var data = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");

                // tries to post a request with the http client
                try {
                    // HttpResponseMessage response;
                    // using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                    //
                    // try {
                    //     response = await httpClient.PostAsync(loginLink, data, tokenSource.Token);
                    // }
                    // catch (TaskCanceledException) {
                    //     throw new TimeoutException(
                    //         $"Error 504: Logging into {link} has timed out");
                    // }
                    var response = await _httpClient.PostAsync(loginLink, data);

                    response.EnsureSuccessStatusCode();

                    return await response.Content.ReadAsStringAsync();
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
}