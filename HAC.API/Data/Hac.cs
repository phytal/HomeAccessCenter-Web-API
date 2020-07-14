using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using HAC.API.Data.Objects;
using HtmlAgilityPack;

namespace HAC.API.Data {
    public static class Hac {
        // contains the headers needed for a http request
        public static readonly Dictionary<string, string> HandlerProperties = new Dictionary<string, string> {
            {"Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8"},
            {"Accept-Language", "en-US,en;q=0.8"},
            {"Accept-Encoding", "gzip, deflate"},
            {"Connection", "keep-alive"},
            {"DNT", "1"},
            {"Upgrade-Insecure-Requests", "1"},
            {"User-Agent", "Chrome/77.0.3865.120"}
        };

        public static async Task<LoginResponse> Login(string link, string username, string password) {
            var loginLink = $"{link}/HomeAccess/Account/LogOn";
            var container = new CookieContainer();
            try {
                // setting up the http client
                using var handler = new HttpClientHandler {CookieContainer = container};
                using var httpClient = new HttpClient(handler) {BaseAddress = new Uri(link)};
                httpClient.DefaultRequestHeaders.Referrer =
                    new Uri($"{link}/HomeAccess/Account/LogOn");
                httpClient.DefaultRequestHeaders.CacheControl = CacheControlHeaderValue.Parse("max-age=0");
                httpClient.DefaultRequestHeaders.ExpectContinue = false;
                httpClient.DefaultRequestHeaders.Add("Origin", @$"{link}/");
                foreach (var (key, value) in HandlerProperties) httpClient.DefaultRequestHeaders.Add(key, value);

                var body = @"Database=10&LogOnDetails.UserName=" + username + "&LogOnDetails.Password=" + password;
                var data = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");

                // tries to post a request with the http client
                try {
                    var response = await httpClient.PostAsync(loginLink, data);

                    response.EnsureSuccessStatusCode();
                    var uri = httpClient.BaseAddress;

                    var responseBody = await response.Content.ReadAsStringAsync();

                    return new LoginResponse {
                        ResponseBody = responseBody,
                        CookieContainer = container,
                        RequestUri = uri
                    };
                }
                catch (HttpRequestException e) {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);

                    return null;
                }
            }
            catch {
                return null;
            }
        }

        public static Response GetAll(CookieContainer cookies, Uri requestUri, string link) {
            Student studentInfo;
            List<List<List<Day>>> calendarList;
            List<List<TranscriptCourse>> oldAssignmentList;
            List<List<AssignmentCourse>> currentAssignmentList;
            List<List<Course>> reportCardList, iprList;
            try {
                //student info
                var studentData = Utils.GetData(cookies, requestUri, link, ResponseType.Registration);
                var studentDataDocument = new HtmlDocument();
                studentDataDocument.LoadHtml(studentData);
                studentInfo = StudentInfo.GetAllStudentInfo(studentDataDocument);

                //attendance 
                //calendarList = Attendance.GetAttendances(cookies, requestUri, link);

                //report card
                var reportCardData = Utils.GetData(cookies, requestUri, link, ResponseType.ReportCards);
                var reportCardHtmlDocument = new HtmlDocument();
                reportCardHtmlDocument.LoadHtml(reportCardData);
                reportCardList = ReportCard.CheckReportCardTask(reportCardHtmlDocument);

                //ipr
                iprList = Ipr.GetGradesFromIpr(cookies, requestUri, link);

                //current courses
                var assignmentList = Courses.GetAssignmentsFromMarkingPeriod(cookies, requestUri, link);
                currentAssignmentList = assignmentList;

                //past courses/transcript 
                var oldData = Utils.GetData(cookies, requestUri, link, ResponseType.Transcript);
                oldAssignmentList = Transcript.GetTranscript(oldData);
            }
            catch (Exception e) {
                Console.WriteLine(e);
                return new Response {
                    Message = $"Error 404: Could not fetch information. Exception: {e}"
                };
            }

            return new Response {
                Message = "Success",
                StudentInfo = studentInfo,
                //Attendances = calendarList,
                AssignmentList = currentAssignmentList,
                TranscriptList = oldAssignmentList,
                ReportCardList = reportCardList,
                IprList = iprList
            };
        }

        public static Response GetStudentInfo(CookieContainer cookies, Uri requestUri, string link) {
            Student studentInfo;
            try {
                var studentData = Utils.GetData(cookies, requestUri, link, ResponseType.Registration);
                var studentDataDocument = new HtmlDocument();
                studentDataDocument.LoadHtml(studentData);
                studentInfo = StudentInfo.GetAllStudentInfo(studentDataDocument);
            }
            catch (Exception e) {
                Console.WriteLine(e);
                return new Response {
                    Message = $"Error 404: Could not fetch information. Exception: {e}"
                };
            }

            return new Response {
                Message = "Success",
                StudentInfo = studentInfo
            };
        }


        public static Response GetCourses(CookieContainer cookies, Uri requestUri, string link) {
            List<List<AssignmentCourse>> currentAssignmentList;
            var assignmentList = Courses.GetAssignmentsFromMarkingPeriod(cookies, requestUri, link);

            try {
                currentAssignmentList = assignmentList;
            }
            catch (Exception e) {
                Console.WriteLine(e);
                return new Response {
                    Message = $"Error 404: Could not fetch information. Exception: {e}"
                };
            }

            return new Response {
                Message = "Success",
                AssignmentList = currentAssignmentList
            };
        }

        public static Response GetIpr(CookieContainer cookies, Uri requestUri, string link) {
            List<List<Course>> iprList;

            try {
                iprList = Ipr.GetGradesFromIpr(cookies, requestUri, link);
            }
            catch (Exception e) {
                Console.WriteLine(e);
                return new Response {
                    Message = $"Error 404: Could not fetch information. Exception: {e}"
                };
            }

            return new Response {
                Message = "Success",
                IprList = iprList
            };
        }

        public static Response GetReportCard(CookieContainer cookies, Uri requestUri, string link) {
            List<List<Course>> reportCardCourses;
            try {
                var reportCardData = Utils.GetData(cookies, requestUri, link, ResponseType.ReportCards);
                var reportCardHtmlDocument = new HtmlDocument();
                reportCardHtmlDocument.LoadHtml(reportCardData);
                reportCardCourses = ReportCard.CheckReportCardTask(reportCardHtmlDocument);
            }
            catch (Exception e) {
                Console.WriteLine(e);
                return new Response {
                    Message = $"Error 404: Could not fetch information. Exception: {e}"
                };
            }

            return new Response {
                Message = "Success",
                ReportCardList = reportCardCourses
            };
        }

        public static Response GetTranscript(CookieContainer cookies, Uri requestUri, string link) {
            List<List<TranscriptCourse>> oldAssignmentList;
            try {
                var oldData = Utils.GetData(cookies, requestUri, link, ResponseType.Transcript);
                oldAssignmentList = Transcript.GetTranscript(oldData);
            }
            catch (Exception e) {
                Console.WriteLine(e);
                return new Response {
                    Message = $"Error 404: Could not fetch information. Exception: {e}"
                };
            }

            return new Response {
                Message = "Success",
                TranscriptList = oldAssignmentList
            };
        }

        public static Response GetAttendance(CookieContainer cookies, Uri requestUri, string link) {
            List<List<List<Day>>> calendarList;

            try {
                calendarList = Attendance.GetAttendances(cookies, requestUri, link);
            }
            catch (Exception e) {
                Console.WriteLine(e);
                return new Response {
                    Message = $"Error 404: Could not fetch information. Exception: {e}"
                };
            }

            return new Response {
                Message = "Success",
                Attendances = calendarList
            };
        }

        public static bool IsValidLogin(string response) {
            return !response.Contains("You have entered an incorrect HAC ID or password");
        }

        public class LoginResponse {
            public string ResponseBody { get; set; }
            public Uri RequestUri { get; set; }
            public CookieContainer CookieContainer { get; set; }
        }
    }
}