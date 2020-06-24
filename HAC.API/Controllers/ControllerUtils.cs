using System.Net;
using HAC.API.Data.Objects;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace HAC.API.Controllers
{
    public static class Utils
    {
        public static Response GetResponse<T>(HttpContext context, ILogger<T> logger)
        {
            string hacLink = context.Request.Query["hacLink"];
            string username = context.Request.Query["username"];
            string password = context.Request.Query["password"];
            
            if (string.IsNullOrEmpty(hacLink)) 
                return new Response { Message = "Error 404: Empty hacLink parameter." }; 
            if (string.IsNullOrEmpty(username)) 
                return new Response { Message = "Error 404: Empty username parameter." }; 
            if (string.IsNullOrEmpty(password))
                return new Response { Message = "Error 404: Empty password parameter." };
            
            logger.LogInformation("Received a request.\n" +
                                   $"Link: {hacLink}\n" +
                                   $"Username: {username}\n" +
                                   $"Password: {password}");
            
            var hac = new Data.Hac();
            HttpWebResponse response = hac.Login(hacLink, username, password, out var container);

            if (!hac.IsValidLogin(response)) //checks if login creds are true
            {
                const string errorText = "Either the HAC username or password is incorrect.";
                return new Response
                {
                    Message = errorText
                };
            }

            var result = new Response();
            var type = logger.GetType().GenericTypeArguments[0];
            
            if (type == typeof(HacController))
            {
                result = hac.GetAll(container, response.ResponseUri, hacLink);
            }
            else if (type == typeof(CourseController))
            {
                result = hac.GetCourses(container, response.ResponseUri, hacLink);
            }
            else if (type == typeof(IprController))
            {
                result = hac.GetIpr(container, response.ResponseUri, hacLink);
            }
            else if (type == typeof(ReportCardController))
            {
                result = hac.GetReportCard(container, response.ResponseUri, hacLink);
            }
            else if (type == typeof(TranscriptController))
            {
                result = hac.GetTranscript(container, response.ResponseUri, hacLink);
            }

            return result;
        }
    }
}