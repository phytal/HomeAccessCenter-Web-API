using HAC.API.Data.Objects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HAC.API.Controllers {
    [Route("api/hac")]
    [ApiController]
    public class HacController : ControllerBase {
        private readonly ILogger<HacController> _logger;

        public HacController(ILogger<HacController> logger) {
            _logger = logger;
        }


        // GET: api/<HacController>
        [HttpGet]
        public Response Get() {
            // Retrieves the username, password, and hacLink values from the URL Request.
            // Then logs in and fetches the result
            var result = Utils.GetResponse(HttpContext, _logger);

            return result;
        }
    }
}