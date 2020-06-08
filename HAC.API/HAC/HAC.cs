using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
            var assignmentList1 = new List<AssignmentCourse>();
            var assignmentList2 = new List<AssignmentCourse>();
            var assignmentList3 = new List<AssignmentCourse>();
            var assignmentList4 = new List<AssignmentCourse>();
            var reportCardCourses = new IEnumerable<Course>[4];
            try
            {
                //report card
                string reportCardData = GetData(cookies, requestUri, link, ResponseType.ReportCards);
                var reportCardHtmlDocument = new HtmlDocument();
                reportCardHtmlDocument.LoadHtml(reportCardData);
                reportCardCourses = CheckReportCard.CheckReportCardTask(reportCardHtmlDocument);
                //current courses
                var assignmentList = GetAssignmentsFromMarkingPeriod(cookies, requestUri, link);
                
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
                string oldData = GetData(cookies, requestUri, link, ResponseType.Transcript);

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
                AssignmentList1 = assignmentList1,
                AssignmentList2 = assignmentList2,
                AssignmentList3 = assignmentList3,
                AssignmentList4 = assignmentList4,
                OldAssignmentList = oldAssignmentList,
                ReportCardList1 = reportCardCourses[0],
                ReportCardList2 = reportCardCourses[1],
                ReportCardList3 = reportCardCourses[2],
                ReportCardList4 = reportCardCourses[3],
            };
        }

        //TODO: move this to a separate endpoint
        private List<List<AssignmentCourse>> GetAssignmentsFromMarkingPeriod(CookieContainer cookies, Uri requestUri, string link)
        {
            //var assignmentList = new List<AssignmentCourse>();
            var assignmentList = new List<List<AssignmentCourse>>();
            var documentList = new List<HtmlDocument>();
            string data = GetData(cookies, requestUri, link, ResponseType.Assignments);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(data);

            var form = new GradesForm(htmlDocument);
            var reportingPeriodNames = form.ReportingPeriodNames();
            foreach (var name in reportingPeriodNames)
            {
                var body = form.GenerateFormBody(name);
                var response = GetDataFromReportingPeriod(cookies, requestUri, link, body);
                var doc = new HtmlDocument();
                doc.LoadHtml(response);
                documentList.Add(doc);
            }

            foreach (var document in documentList)
            {
                var localAssignmentList = new List<AssignmentCourse>();
                var courseHtml = document.DocumentNode.Descendants("div")
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

                    localAssignmentList.Add(new AssignmentCourse
                    {
                        CourseName = courseName, CourseId = courseId, CourseAverage = double.Parse(courseGrade),
                        Assignments = new List<Assignment>()
                    });

                }
                assignmentList.Add(localAssignmentList);
            }

            return assignmentList;
        }


        private string GetData(CookieContainer cookies, Uri requestUri, string link, ResponseType type)
        {
            string s = string.Empty;
            foreach (Cookie cookie in cookies.GetCookies(requestUri))
            {
                s += (cookie.Name + "=" + cookie.Value + "; ");
            }

            try
            {
                HttpWebRequest request =
                    (HttpWebRequest) WebRequest.Create(new Uri($"{link}/HomeAccess/Content/Student/{type.ToString()}.aspx"));

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
        
        private string GetDataFromReportingPeriod(CookieContainer cookies,  Uri requestUri, string link, string body)
        {
            string s = string.Empty;
            foreach (Cookie cookie in cookies.GetCookies(requestUri))
            {
                s += (cookie.Name + "=" + cookie.Value + "; ");
            }
            try
            {
                HttpWebRequest request = (HttpWebRequest) WebRequest.Create(
                    $"{link}/HomeAccess/Content/Student/Assignments.aspx");

                request.KeepAlive = true;
                request.Headers.Set(HttpRequestHeader.CacheControl, "max-age=0");
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
                request.Headers.Add("Origin", $"{link}");
                request.Headers.Add("Upgrade-Insecure-Requests", @"1");
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.61 Safari/537.36";
                request.ContentType = "application/x-www-form-urlencoded";
                request.Referer = $"{link}/HomeAccess/Content/Student/Assignments.aspx";
                request.Headers.Set(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
                request.Headers.Set(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.9");
                request.Headers.Set(HttpRequestHeader.Cookie, s);
                
                request.Method = "POST";
                request.ServicePoint.Expect100Continue = false;
                
                byte[] postBytes = Encoding.UTF8.GetBytes(body);
                request.ContentLength = postBytes.Length;
                Stream stream = request.GetRequestStream();
                stream.Write(postBytes, 0, postBytes.Length);
                
                var result = ReadResponse((HttpWebResponse)request.GetResponse());
                return result;
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

    public enum ResponseType
    {
        Transcript,
        ReportCards,
        Assignments
    }
}