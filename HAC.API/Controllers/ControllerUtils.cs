using HAC.API.Data;
using HAC.API.Data.Objects;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace HAC.API.Controllers {
    public interface IControllerUtils {
        Response GetResponse<T>(HttpContext context, ILogger<T> logger);
    }

    public class ControllerUtils : IControllerUtils {
        private readonly IHac _hac;
        private readonly ILogin _login;

        public ControllerUtils(IHac hac, ILogin login) {
            _hac = hac;
            _login = login;
        }

        public Response GetResponse<T>(HttpContext context, ILogger<T> logger) {
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

            var response = _login.LoginAsync(hacLink, username, password);

            if (!_hac.IsValidLogin(response.Result)) //checks if login credentials are true
            {
                const string errorText = "Error 401: Either the HAC username or password is incorrect.";
                return new Response {
                    Message = errorText
                };
            }

            var result = new Response();
            var type = logger.GetType().GenericTypeArguments[0];

            if (type == typeof(HacController))
                result = _hac.GetAll(hacLink);
            else if (type == typeof(StudentController))
                result = _hac.GetStudentInfo(hacLink);
            else if (type == typeof(CourseController))
                result = _hac.GetCourses(hacLink);
            else if (type == typeof(IprController))
                result = _hac.GetIpr(hacLink);
            else if (type == typeof(ReportCardController))
                result = _hac.GetReportCard(hacLink);
            else if (type == typeof(TranscriptController))
                result = _hac.GetTranscript(hacLink);
            else if (type == typeof(AttendanceController)) result = _hac.GetAttendance(hacLink);

            return result;
        }
    }
}