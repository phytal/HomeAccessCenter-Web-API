using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;

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

        public static string GetDataFromReportingPeriod(CookieContainer cookies, Uri requestUri, string link, string body)
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
    }

    public enum ResponseType
    {
        Transcript,
        ReportCards,
        Assignments
    }
}
