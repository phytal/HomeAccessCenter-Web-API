using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace HAC.API.Data
{
    public static class Utils
    {
        public static string GetData(CookieContainer cookies, Uri requestUri, string link, ResponseType type)
        {
            string s = string.Empty;
            foreach (Cookie cookie in cookies.GetCookies(requestUri))
            {
                s += (cookie.Name + "=" + cookie.Value + "; ");
            }

            try
            {
                HttpWebRequest request =
                    (HttpWebRequest) WebRequest.Create(
                        new Uri($"{link}/HomeAccess/Content/Student/{type.ToString()}.aspx"));

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

        public static string GetDataFromReportingPeriod(CookieContainer cookies, Uri requestUri, string link,
            string body)
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
                request.Accept =
                    "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
                request.Headers.Add("Origin", $"{link}");
                request.Headers.Add("Upgrade-Insecure-Requests", @"1");
                request.UserAgent =
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.61 Safari/537.36";
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

                var result = ReadResponse((HttpWebResponse) request.GetResponse());
                return result;
            }
            catch
            {
                return null;
            }
        }

        public static string GetDataFromIprDate(CookieContainer cookies, Uri requestUri, string link, string body)
        {
            string s = string.Empty;
            foreach (Cookie cookie in cookies.GetCookies(requestUri))
            {
                s += (cookie.Name + "=" + cookie.Value + "; ");
            }

            try
            {
                HttpWebRequest request = (HttpWebRequest) WebRequest.Create(
                    $"{link}/HomeAccess/Content/Student/InterimProgress.aspx");

                request.KeepAlive = true;
                request.Headers.Set(HttpRequestHeader.CacheControl, "max-age=0");
                request.Accept =
                    "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
                request.Headers.Add("Origin", $"{link}");
                request.Headers.Add("Upgrade-Insecure-Requests", @"1");
                request.UserAgent =
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.61 Safari/537.36";
                request.ContentType = "application/x-www-form-urlencoded";
                request.Referer = $"{link}/HomeAccess/Content/Student/InterimProgress.aspx";
                request.Headers.Set(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
                request.Headers.Set(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.9");
                request.Headers.Set(HttpRequestHeader.Cookie, s);

                request.Method = "POST";
                request.ServicePoint.Expect100Continue = false;

                byte[] postBytes = Encoding.UTF8.GetBytes(body);
                request.ContentLength = postBytes.Length;
                Stream stream = request.GetRequestStream();
                stream.Write(postBytes, 0, postBytes.Length);

                var result = ReadResponse((HttpWebResponse) request.GetResponse());
                return result;
            }
            catch
            {
                return null;
            }
        }

        public static string ReadResponse(HttpWebResponse response)
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

        /// <summary>
        /// Returns the course information
        /// </summary>
        /// <param name="courseName">Course Name</param>
        /// <param name="courseId">Course Id</param>
        /// <returns>Returns course name, course id</returns>
        public static Tuple<string, string> BeautifyCourseInfo(string courseName = null, string courseId = null)
        {
            if (courseName != null)
            {
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
            }

            if (courseId != null)
            {
                courseId = courseId.Remove(courseId.Length - 4);
                //removes excess
                while (courseId.LastOrDefault() == ' ' || courseId.LastOrDefault() == '-' ||
                       courseId.LastOrDefault() == 'A' || courseId.LastOrDefault() == 'B')
                {
                    courseId = courseId.TrimEnd(courseId[^1]);

                }
            }

            return new Tuple<string, string>(courseName, courseId);
        }
        
        public static string FormatName(string fullName)
        {
            var firstMiddleName = fullName.Split(',')[1].Trim().ToLower();
            var fmName = firstMiddleName.Split(' ');
            var firstNameBuilder = new StringBuilder();
            foreach (var name in fmName)
            {
                firstNameBuilder.Append(char.ToUpper(name[0]) + name.Substring(1) + " ");
            }

            var firstName = firstNameBuilder.ToString().TrimEnd(' ');
            var lastName = fullName.Split(',')[0].Trim().ToLower();
            lastName = char.ToUpper(lastName[0]) + lastName.Substring(1);
            fullName = firstName + " " + lastName;
            
            return fullName;
        }
    }

    public enum ResponseType
    {
        Transcript,
        ReportCards,
        Assignments,
        InterimProgress,
        Registration
    }
}
