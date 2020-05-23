using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using HAC_API.HAC.Objects;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HAC_API.Controllers
{
    [Route("api/test")]
    [ApiController]
    public class HacController : ControllerBase
    {

        private readonly ILogger<HacController> _logger;

        public HacController(ILogger<HacController> logger)
        {
            _logger = logger;
        }


        // GET: api/<HacController>
        [HttpGet]
        public Response Get()
        {
            // Retrieve the value of "username" from the URL Request.
            string username = HttpContext.Request.Query["username"];
            string password = HttpContext.Request.Query["password"];
            if (string.IsNullOrEmpty(username)) 
                return new Response { Message = "Error 404: Empty username parameter." };
            else if (string.IsNullOrEmpty(password))
                return new Response { Message = "Error 404: Empty password parameter." };

            _logger.LogInformation("Recieved a request.\n" +
                $"Username: {username}\n" +
                $"Password: {password}");

            var hac = new HAC.HAC();
            HttpWebResponse response = hac.Login(username, password, out var container);

            if (!hac.isValidLogin(response)) //checks if login creds are true
            {
                string errorText = "Either the HAC username or password is incorrect.";
                return new Response
                {
                    Message = errorText
                };
            }
            var result = hac.GetCourses(container, response.ResponseUri);//logs in and fetches grades

            return result;
        }
    }
}
