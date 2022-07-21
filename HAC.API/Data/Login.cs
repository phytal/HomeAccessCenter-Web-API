using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Sentry;
using HtmlAgilityPack;
using System.Linq;

namespace HAC.API.Data {
    public interface ILogin {
        Task<string> LoginAsync(string link, string username, string password);
    }

    public class Login : ILogin {
        // contains the headers needed for a http request
        public static readonly Dictionary<string, string> HandlerProperties = new Dictionary<string, string> {
            {"Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8"},
            {"Accept-Language", "en-US,en;q=0.5"},
            {"Accept-Encoding", "gzip, deflate"},
            {"Connection", "keep-alive"},
            {"DNT", "1"},
            {"Upgrade-Insecure-Requests", "1"}, {
                "User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:88.0) Gecko/20100101 Firefox/88.0"
            }
        };
        private readonly string _requestVerificationToken;
        private readonly HttpClient _httpClient;

        public Login(HttpClient httpClient) {
            _httpClient = httpClient;
        }

        public async Task<string> LoginAsync(string link, string username, string password) {
            try {
                link = $"https://hac.friscoisd.org/HomeAccess/Account/LogOn?ReturnUrl=%2fHomeAccess%2f";

                _httpClient.BaseAddress = new Uri(link);
                _httpClient.DefaultRequestHeaders.Referrer =
                    new Uri($"{link}/HomeAccess/Account/LogOn?ReturnUrl=%2fHomeAccess%2fClasses%2fClasswork");
                _httpClient.DefaultRequestHeaders.CacheControl = CacheControlHeaderValue.Parse("max-age=0");
                _httpClient.DefaultRequestHeaders.ExpectContinue = false;
                _httpClient.DefaultRequestHeaders.Add("Origin", @$"{link}/");
                foreach (var (key, value) in HandlerProperties) _httpClient.DefaultRequestHeaders.Add(key, value);

                string requestVerificationToken = await getRequestVerificationToken(link);

                var body = @"Database=10&VerificationOption=UsernamePassword&LogOnDetails.UserName=" + username + "&LogOnDetails.Password=" + password + "&__RequestVerificationToken=" + requestVerificationToken;
                var data = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");

                // tries to post a request with the http client
                try {
                    var response = await _httpClient.PostAsync(link, data);
                    Console.WriteLine(response);
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

        private async Task<string> getRequestVerificationToken(string link) {
            var htmlDocument = new HtmlDocument();
            var html = await _httpClient.GetAsync(link).Result.Content.ReadAsStringAsync();
            htmlDocument.LoadHtml(html);

            return htmlDocument.DocumentNode.Descendants("input")
                .FirstOrDefault(node => node.GetAttributeValue("name", "")
                    .Equals("__RequestVerificationToken")).GetAttributeValue("value", "").Trim();
        }
    }
}