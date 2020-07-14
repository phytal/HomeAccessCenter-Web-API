using HAC.API.Data.Objects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HAC.API.Controllers {
    [Route("api/ipr")]
    [ApiController]
    public class IprController : ControllerBase {
        private readonly ILogger<IprController> _logger;

        public IprController(ILogger<IprController> logger) {
            _logger = logger;
        }


        // GET: api/<IprController>
        [HttpGet]
        public Response Get() {
            // Retrieves the username, password, and hacLink values from the URL Request.
            // Then logs in and fetches the result
            var result = Utils.GetResponse(HttpContext, _logger);

            return result;
        }
    }
}