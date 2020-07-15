using HAC.API.Data;
using HAC.API.Data.Objects;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace HAC.API.Controllers {
    public static class Utils {
        public static Response GetResponse<T>(HttpContext context, ILogger<T> logger) {
            string hacLink = context.Request.Query["hacLink"];
            string username = context.Request.Query["username"];
            string password = context.Request.Query["password"];

            if (string.IsNullOrEmpty(hacLink))
                return new Response {Message = "Error 404: Empty hacLink parameter."};
            if (string.IsNullOrEmpty(username))
                return new Response {Message = "Error 404: Empty username parameter."};
            if (string.IsNullOrEmpty(password))
                return new Response {Message = "Error 404: Empty password parameter."};

            logger.LogInformation("Received a request.\n" +
                                  $"Link: {hacLink}\n" +
                                  $"Username: {username}\n" +
                                  $"Password: {password}");

            var response = Hac.Login(hacLink, username, password);
            var container = response.Result.CookieContainer;
            var uri = response.Result.RequestUri;

            if (!Hac.IsValidLogin(response.Result.ResponseBody)) //checks if login credentials are true
            {
                const string errorText = "Error 401: Either the HAC username or password is incorrect.";
                return new Response {
                    Message = errorText
                };
            }

            var result = new Response();
            var type = logger.GetType().GenericTypeArguments[0];

            if (type == typeof(HacController))
                result = Hac.GetAll(container, uri, hacLink);
            else if (type == typeof(StudentController))
                result = Hac.GetStudentInfo(container, uri, hacLink);
            else if (type == typeof(CourseController))
                result = Hac.GetCourses(container, uri, hacLink);
            else if (type == typeof(IprController))
                result = Hac.GetIpr(container, uri, hacLink);
            else if (type == typeof(ReportCardController))
                result = Hac.GetReportCard(container, uri, hacLink);
            else if (type == typeof(TranscriptController))
                result = Hac.GetTranscript(container, uri, hacLink);
            else if (type == typeof(AttendanceController)) result = Hac.GetAttendance(container, uri, hacLink);

            return result;
        }
    }
}