using HAC.API.Data.Objects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HAC.API.Controllers {
    [Route("api/hac")]
    [ApiController]
    public class HacController : ControllerBase {
        private readonly IControllerUtils _controllerUtils;
        private readonly ILogger<HacController> _logger;

        public HacController(ILogger<HacController> logger, IControllerUtils controllerUtils) {
            _logger = logger;
            _controllerUtils = controllerUtils;
        }

        // GET: api/<HacController>
        [HttpGet]
        public Response Get() {
            // Retrieves the username, password, and hacLink values from the URL Request.
            // Then logs in and fetches the result
            var result = _controllerUtils.GetResponse(HttpContext, _logger);

            return result;
        }
    }
}