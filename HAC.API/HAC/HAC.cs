using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using HAC.API.HAC.Objects;
using HtmlAgilityPack;

namespace HAC.API.HAC
{
    public class HAC
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

        public Response GetCourses(CookieContainer cookies, Uri requestUri, string link)
        {
            var oldAssignmentList = new List<Course>();
            var assignmentList = new List<AssignmentCourse>();
            var reportCardCourses = new IEnumerable<Course>[4];
            try
            {
                //report card
                string reportCardData = GetRawReportCardData(cookies, requestUri, link);
                var reportCardHtmlDocument = new HtmlDocument();
                reportCardHtmlDocument.LoadHtml(reportCardData);
                reportCardCourses = CheckReportCard.CheckReportCardTask(reportCardHtmlDocument);
                //current courses
                string data = GetRawGradeData(cookies, requestUri, link);

                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(data);

                var courseHtml = htmlDocument.DocumentNode.Descendants("div")
                    .Where(node => node.GetAttributeValue("class", "")
                        .Equals("AssignmentClass")).ToList();
                Regex x = new Regex(@"\w+\s-\s\d\s");

                foreach (var courseHtmlItem in courseHtml)
                {
                    var course = courseHtmlItem.Descendants("a")
                        .FirstOrDefault(node => node.GetAttributeValue("class", "")
                            .Equals("sg-header-heading")).InnerText.Trim();
                    var courseName = x.Replace(course, @"").Trim();
                    //removes semester 
                    while (courseName.Substring(courseName.Length - 2) == "S1" ||
                           courseName.Substring(courseName.Length - 2) == "S2")
                    {
                        courseName = courseName.Replace(courseName.Substring(courseName.Length - 2), "");
                        while (courseName.LastOrDefault() == ' ' || courseName.LastOrDefault() == '-')
                        {
                            courseName = courseName.TrimEnd(courseName[^1]);
                        }
                    }

                    var courseId = x.Match(course).ToString().Trim();
                    courseId = courseId.Remove(courseId.Length - 4);
                    //removes excess
                    while (courseId.LastOrDefault() == ' ' || courseId.LastOrDefault() == '-' ||
                           courseId.LastOrDefault() == 'A' || courseId.LastOrDefault() == 'B')
                    {
                        courseId = courseId.TrimEnd(courseId[^1]);
                    }

                    string courseGrade;
                    try
                    {
                        courseGrade = courseHtmlItem.Descendants("span")
                            .FirstOrDefault(node => node.GetAttributeValue("class", "")
                                .Equals("sg-header-heading sg-right"))?.InnerText.Trim().Remove(0, 15)
                            .TrimEnd('%');
                    }
                    catch
                    {
                        continue;
                    }

                    assignmentList.Add(new AssignmentCourse()
                        {CourseName = courseName, CourseId = courseId, CourseAverage = double.Parse(courseGrade), Assignments = new List<Assignment>()});
                }

                //past courses 
                string oldData = GetRawOldGradeData(cookies, requestUri, link);

                var oldHtmlDocument = new HtmlDocument();
                oldHtmlDocument.LoadHtml(oldData); //gets all of the years
                byte numCourses = 0; //no one will exceed 255 
                for (byte i = 0; i >= 0; i++)
                    //what this does is tries to get as many courses as possible until it receives an error, then pass on that number to get that number of courses
                {
                    try //get number of years
                    {
                        var oldCourseHtml = oldHtmlDocument.DocumentNode
                            .Descendants("table") //initializes variable but isn't used purposely for testing purposes
                            .Where(node => node.GetAttributeValue("id", "")
                                .Equals($"plnMain_rpTranscriptGroup_dgCourses_{i}")).ToList();
                        if (oldCourseHtml.Count == 0) break;
                    }
                    catch (Exception)
                    {
                        break;
                    }

                    numCourses = i;
                }

                //note: numCourses will always be 1 less because of indexes
                for (byte i = 0; i <= numCourses - 1; i++) //get all years (-1 to exclude the present year)
                {
                    var oldCourseHtml = oldHtmlDocument.DocumentNode.Descendants("table")
                        .Where(node => node.GetAttributeValue("id", "")
                            .Equals($"plnMain_rpTranscriptGroup_dgCourses_{i}")).ToList();
                    var oldCourseItemList = oldCourseHtml[0].Descendants("tr")
                        .Where(node => node.GetAttributeValue("class", "")
                            .Equals($"sg-asp-table-data-row")).ToList();
                    foreach (var courseHtmlItem in oldCourseItemList) //foreach course that is listed in a year
                    {
                        var courseName = courseHtmlItem.Descendants("td") //gets course name
                            .ElementAt(1).InnerText
                            .Trim(); //course name is stored at the second instance of td

                        var courseId = courseHtmlItem.Descendants("td") //gets course id
                            .ElementAt(0).InnerText
                            .Trim(); //course name is stored at the first instance of td

                        courseId = courseId.Remove(courseId.Length - 4);

                        while (courseId.LastOrDefault() == ' ' || courseId.LastOrDefault() == '-' ||
                               courseId.LastOrDefault() == 'A' || courseId.LastOrDefault() == 'B' ||
                               courseId.LastOrDefault() == 'Y' || courseId.LastOrDefault() == 'M')
                        {
                            courseId = courseId.TrimEnd(courseId[^1]);
                        }

                        for (byte j = 4; j <= 4 && j > 1; j--)
                            //gets grade, starts from last element, which is overall avg, if nothing, goes to second semester, then first semester
                        {
                            var courseGrade = ""; //finalized course grade
                            var courseGradeHtml = courseHtmlItem.Descendants("td") //gets course grade
                                .ElementAt(j).InnerText;

                            if (courseGradeHtml == "&nbsp;")
                            {
                                continue; //if it is not a grade and is empty then retry
                            }

                            if (j == 3)
                                break; //if the course avg is available then you dont need to get the semester grades
                            courseGrade = courseGradeHtml;
                            oldAssignmentList.Add(new Course
                            {
                                CourseName = courseName,
                                CourseId = courseId,
                                CourseAverage = double.Parse(courseGrade)
                            }); //turns the grade (string) received into a double 
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
                Message = "Success.",
                CurrentAssignmentList = assignmentList,
                OldAssignmentList = oldAssignmentList,
                ReportCardList1 = reportCardCourses[0],
                ReportCardList2 = reportCardCourses[1],
                ReportCardList3 = reportCardCourses[2],
                ReportCardList4 = reportCardCourses[3],
            };
        }

        private string GetRawGradeData(CookieContainer cookies, Uri requestUri, string link)
        {
            string s = string.Empty;
            foreach (Cookie cookie in cookies.GetCookies(requestUri))
            {
                s += (cookie.Name + "=" + cookie.Value + "; ");
            }

            try
            {
                HttpWebRequest request =
                    (HttpWebRequest) WebRequest.Create(new Uri($"{link}/HomeAccess/Content/Student/Assignments.aspx"));

                request.KeepAlive = true;
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                request.Headers.Add("Upgrade-Insecure-Requests", @"1");
                request.UserAgent = "Chrome/77.0.3865.120";
                request.Headers.Set(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
                request.Headers.Set(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.8");
                request.Headers.Set(HttpRequestHeader.Cookie, s);
                return ReadResponse((HttpWebResponse) request.GetResponse());
            }
            catch
            {
                return null;
            }
        }

        private string GetRawOldGradeData(CookieContainer cookies, Uri requestUri, string link)
        {
            string s = string.Empty;
            foreach (Cookie cookie in cookies.GetCookies(requestUri))
            {
                s += (cookie.Name + "=" + cookie.Value + "; ");
            }

            try
            {
                HttpWebRequest request =
                    (HttpWebRequest) WebRequest.Create(new Uri($"{link}/HomeAccess/Content/Student/Transcript.aspx"));

                request.KeepAlive = true;
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                request.Headers.Add("Upgrade-Insecure-Requests", @"1");
                request.UserAgent = "Chrome/77.0.3865.120";
                request.Headers.Set(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
                request.Headers.Set(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.8");
                request.Headers.Set(HttpRequestHeader.Cookie, s);
                return ReadResponse((HttpWebResponse) request.GetResponse());
            }
            catch
            {
                return null;
            }
        }

        private string GetRawReportCardData(CookieContainer cookies, Uri requestUri, string link)
        {
            string s = string.Empty;
            foreach (Cookie cookie in cookies.GetCookies(requestUri))
            {
                s += (cookie.Name + "=" + cookie.Value + "; ");
            }

            try
            {
                HttpWebRequest request =
                    (HttpWebRequest) WebRequest.Create(new Uri($"{link}/HomeAccess/Content/Student/ReportCards.aspx"));

                request.KeepAlive = true;
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                request.Headers.Add("Upgrade-Insecure-Requests", @"1");
                request.UserAgent = "Chrome/77.0.3865.120";
                request.Headers.Set(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
                request.Headers.Set(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.8");
                request.Headers.Set(HttpRequestHeader.Cookie, s);
                return ReadResponse((HttpWebResponse) request.GetResponse());
            }
            catch
            {
                return null;
            }
        }

        public bool IsValidLogin(HttpWebResponse response)
        {
            return !ReadResponse(response).Contains("You have entered an incorrect HAC ID or password");
        }

        private string ReadResponse(HttpWebResponse response)
        {
            using Stream responseStream = response.GetResponseStream();
            Stream streamToRead = responseStream;
            if (response.ContentEncoding.ToLower().Contains("gzip"))
            {
                streamToRead = new GZipStream(streamToRead, CompressionMode.Decompress);
            }
            else if (response.ContentEncoding.ToLower().Contains("deflate"))
            {
                streamToRead = new DeflateStream(streamToRead, CompressionMode.Decompress);
            }

            using StreamReader streamReader = new StreamReader(streamToRead, Encoding.UTF8);
            return streamReader.ReadToEnd();
        }
    }
}