using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using HAC.API.Data.Objects;
using HtmlAgilityPack;

namespace HAC.API.Data
{
    public class Hac
    {
        public HttpWebResponse Login(string link, string username, string password, out CookieContainer container)
        {
            container = new CookieContainer();
            try
            {
                HttpWebRequest request = (HttpWebRequest) WebRequest.Create(
                    $"{link}/HomeAccess/Account/LogOn?ReturnUrl=%2fHomeAccess%2fClasses%2fClasswork");

                request.KeepAlive = true;
                request.Headers.Set(HttpRequestHeader.CacheControl, "max-age=0");
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                request.Headers.Add("Origin", $"{link}/");
                request.Headers.Add("Upgrade-Insecure-Requests", @"1");
                request.UserAgent = "Chrome/77.0.3865.120";
                request.ContentType = "application/x-www-form-urlencoded";
                request.Referer = $"{link}/HomeAccess/Account/LogOn?ReturnUrl=%2fHomeAccess%2fClasses%2fClasswork";
                request.Headers.Set(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
                request.Headers.Set(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.8");
                request.CookieContainer = container;
                request.Method = "POST";
                request.ServicePoint.Expect100Continue = false;

                string body = @"Database=10&LogOnDetails.UserName=" + username + "&LogOnDetails.Password=" + password;
                byte[] postBytes = Encoding.UTF8.GetBytes(body);
                request.ContentLength = postBytes.Length;
                Stream stream = request.GetRequestStream();
                stream.Write(postBytes, 0, postBytes.Length);
                stream.Close();

                return (HttpWebResponse) request.GetResponse();
            }
            catch
            {
                return null;
            }
        }

        public Response GetAll(CookieContainer cookies, Uri requestUri, string link)
        {
            var oldAssignmentList = new List<Course>();
            var assignmentList1 = new List<AssignmentCourse>();
            var assignmentList2 = new List<AssignmentCourse>();
            var assignmentList3 = new List<AssignmentCourse>();
            var assignmentList4 = new List<AssignmentCourse>();
            var reportCardCourses = new IEnumerable<Course>[4];
            try
            {
                //report card
                string reportCardData = Utils.GetData(cookies, requestUri, link, ResponseType.ReportCards);
                var reportCardHtmlDocument = new HtmlDocument();
                reportCardHtmlDocument.LoadHtml(reportCardData);
                reportCardCourses = ReportCard.CheckReportCardTask(reportCardHtmlDocument);
                //current courses
                var assignmentList = Courses.GetAssignmentsFromMarkingPeriod(cookies, requestUri, link);

                assignmentList1 = assignmentList[0];
                if (assignmentList.Count > 1)
                {
                    assignmentList2 = assignmentList[1];
                    if (assignmentList.Count > 2)
                    {
                        assignmentList3 = assignmentList[2];
                        if (assignmentList.Count > 3)
                        {
                            assignmentList4 = assignmentList[3];
                        }
                    }
                }

                //past courses/transcript 
                string oldData = Utils.GetData(cookies, requestUri, link, ResponseType.Transcript);
                oldAssignmentList = Transcript.GetTranscript(oldData);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new Response
                {
                    Message = $"Error 404: Could not fetch information. Exception: {e}"
                };
            }

            return new Response
            {
                Message = "Success",
                AssignmentList1 = assignmentList1,
                AssignmentList2 = assignmentList2,
                AssignmentList3 = assignmentList3,
                AssignmentList4 = assignmentList4,
                TranscriptList = oldAssignmentList,
                ReportCardList1 = reportCardCourses[0],
                ReportCardList2 = reportCardCourses[1],
                ReportCardList3 = reportCardCourses[2],
                ReportCardList4 = reportCardCourses[3],
            };
        }

        public Response GetCourses(CookieContainer cookies, Uri requestUri, string link)
        {
            var assignmentList1 = new List<AssignmentCourse>();
            var assignmentList2 = new List<AssignmentCourse>();
            var assignmentList3 = new List<AssignmentCourse>();
            var assignmentList4 = new List<AssignmentCourse>();
            var assignmentList = Courses.GetAssignmentsFromMarkingPeriod(cookies, requestUri, link);

            try
            {
                assignmentList1 = assignmentList[0];
                if (assignmentList.Count > 1)
                {
                    assignmentList2 = assignmentList[1];
                    if (assignmentList.Count > 2)
                    {
                        assignmentList3 = assignmentList[2];
                        if (assignmentList.Count > 3)
                        {
                            assignmentList4 = assignmentList[3];
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new Response
                {
                    Message = $"Error 404: Could not fetch information. Exception: {e}"
                };
            }

            return new Response
            {
                Message = "Success",
                AssignmentList1 = assignmentList1,
                AssignmentList2 = assignmentList2,
                AssignmentList3 = assignmentList3,
                AssignmentList4 = assignmentList4,
            };
        }

        public Response GetReportCard(CookieContainer cookies, Uri requestUri, string link)
        {
            var reportCardCourses = new IEnumerable<Course>[4];
            try
            {
                string reportCardData = Utils.GetData(cookies, requestUri, link, ResponseType.ReportCards);
                var reportCardHtmlDocument = new HtmlDocument();
                reportCardHtmlDocument.LoadHtml(reportCardData);
                reportCardCourses = ReportCard.CheckReportCardTask(reportCardHtmlDocument);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new Response
                {
                    Message = $"Error 404: Could not fetch information. Exception: {e}"
                };
            }

            return new Response
            {
                Message = "Success",
                ReportCardList1 = reportCardCourses[0],
                ReportCardList2 = reportCardCourses[1],
                ReportCardList3 = reportCardCourses[2],
                ReportCardList4 = reportCardCourses[3],
            };
        }

        public Response GetTranscript(CookieContainer cookies, Uri requestUri, string link)
        {
            var oldAssignmentList = new List<Course>();
            try
            {
                string oldData = Utils.GetData(cookies, requestUri, link, ResponseType.Transcript);
                oldAssignmentList = Transcript.GetTranscript(oldData);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new Response
                {
                    Message = $"Error 404: Could not fetch information. Exception: {e}"
                };
            }
            return new Response
            {
                Message = "Success",
                TranscriptList = oldAssignmentList,
            };
        }

        public bool IsValidLogin(HttpWebResponse response)
        {
            return !Utils.ReadResponse(response).Contains("You have entered an incorrect HAC ID or password");
        }
    }
}