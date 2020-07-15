using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sentry;

namespace HAC.API.Data {
    public static class Utils {
        public static async Task<string> GetData(CookieContainer cookies, Uri requestUri, string link, ResponseType type,
            string section = "Student", string param = "") {
            var s = string.Empty;
            foreach (Cookie cookie in cookies.GetCookies(requestUri)) s += cookie.Name + "=" + cookie.Value + "; ";

            var requestLink = $"{link}/HomeAccess/Content/{section}/{type.ToString()}.aspx{param}";
            try {
                // setting up the http client
                var handler = new HttpClientHandler {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                    CookieContainer = cookies
                }; 
                var httpClient = new HttpClient(handler) {BaseAddress = new Uri(link)};
                httpClient.DefaultRequestHeaders.Referrer =
                    new Uri(requestLink);
                foreach (var (key, value) in Hac.HandlerProperties) httpClient.DefaultRequestHeaders.Add(key, value);
                
                // tries to post a request with the http client
                try {
                    // TODO: get cancellation tokens to work
                    // HttpResponseMessage response;
                    // using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                    //
                    // try {
                    //     response = await httpClient.GetAsync(link, tokenSource.Token);
                    // }
                    // catch (TaskCanceledException) {
                    //     throw new TimeoutException(
                    //         $"Error 504: Data fetching request to {link} while fetching the {type.ToString()} has timed out.");
                    // }
                    
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

        public static async Task<string> GetDataWithBody(CookieContainer cookies, Uri requestUri, string link, ResponseType type,
            string body, string section = "Student") {
            var s = string.Empty;
            foreach (Cookie cookie in cookies.GetCookies(requestUri)) s += cookie.Name + "=" + cookie.Value + "; ";

            try {
                // setting up the http client
                var handler = new HttpClientHandler {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                    CookieContainer = cookies
                }; 
                var httpClient = new HttpClient(handler) {BaseAddress = new Uri(link)};
                httpClient.DefaultRequestHeaders.Referrer =
                    new Uri($"{link}/HomeAccess/Content/{section}/{type.ToString()}.aspx");
                httpClient.DefaultRequestHeaders.CacheControl = CacheControlHeaderValue.Parse("max-age=0");
                httpClient.DefaultRequestHeaders.ExpectContinue = false;
                httpClient.DefaultRequestHeaders.Add("Origin", @$"{link}/");
                foreach (var (key, value) in Hac.HandlerProperties) httpClient.DefaultRequestHeaders.Add(key, value);
                
                var data = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");

                // tries to post a request with the http client
                try {
                    // HttpResponseMessage response;
                    // using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                    //
                    // try {
                    //     response = await httpClient.PostAsync(link, data, tokenSource.Token);
                    // }
                    // catch (TaskCanceledException) {
                    //     throw new TimeoutException(
                    //         $"Error 504: Data fetching request to {link} while fetching the {type.ToString()} has timed out.");
                    // }
                    //
                    var response = await httpClient.PostAsync(link, data);

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

        public static string PercentEncoder(string s) {
            var reservedCharacters = new Dictionary<string, string> {
                {"!", "%21"},
                {"#", "%23"},
                {"$", "%24"},
                {"%", "%25"},
                {"&", "%26"},
                {"'", "%27"},
                {"(", "%28"},
                {")", "%29"},
                {"*", "%2A"},
                {"+", "%2B"},
                {",", "%2C"},
                {"/", "%2F"},
                {":", "%3A"},
                {";", "%3B"},
                {"=", "%3D"},
                {"?", "%3F"},
                {"@", "%40"},
                {"[", "%5B"},
                {"]", "%5D"},
                {" ", "+"}
            };
            foreach (var character in reservedCharacters.Keys) s = s.Replace(character, reservedCharacters[character]);

            return s;
        }

        /// <summary>
        ///     Returns the course information
        /// </summary>
        /// <param name="courseName">Course Name</param>
        /// <param name="courseId">Course Id</param>
        /// <returns>Returns course name, course id</returns>
        public static Tuple<string, string> BeautifyCourseInfo(string courseName = null, string courseId = null) {
            if (courseName != null) //removes semester 
                while (courseName.Substring(courseName.Length - 2) == "S1" ||
                       courseName.Substring(courseName.Length - 2) == "S2") {
                    courseName = courseName.Replace(courseName.Substring(courseName.Length - 2), "");
                    while (courseName.LastOrDefault() == ' ' || courseName.LastOrDefault() == '-')
                        courseName = courseName.TrimEnd(courseName[^1]);
                }

            if (courseId != null) {
                courseId = courseId.Remove(courseId.Length - 4);
                //removes excess
                while (courseId.LastOrDefault() == ' ' || courseId.LastOrDefault() == '-' ||
                       courseId.LastOrDefault() == 'A' || courseId.LastOrDefault() == 'B')
                    courseId = courseId.TrimEnd(courseId[^1]);
            }

            return new Tuple<string, string>(courseName, courseId);
        }

        public static string FormatName(string fullName, bool formal) {
            var firstMiddleName = fullName.Split(',')[1].Trim().ToLower();
            var fmName = firstMiddleName.Split(' ');
            var firstNameBuilder = new StringBuilder();
            foreach (var name in fmName) firstNameBuilder.Append(char.ToUpper(name[0]) + name.Substring(1) + " ");

            var firstName = firstNameBuilder.ToString().TrimEnd(' ');
            var lastName = fullName.Split(',')[0].Trim().ToLower();
            lastName = char.ToUpper(lastName[0]) + lastName.Substring(1);
            if (formal)
                fullName = lastName + ", " + firstName;
            else
                fullName = firstName + " " + lastName;

            return fullName;
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