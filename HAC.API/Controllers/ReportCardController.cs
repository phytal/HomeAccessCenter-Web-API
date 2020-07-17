using HAC.API.Data.Objects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HAC.API.Controllers {
    [Route("api/reportCard")]
    [ApiController]
    public class ReportCardController : ControllerBase {
        private readonly IControllerUtils _controllerUtils;
        private readonly ILogger<ReportCardController> _logger;

        public ReportCardController(ILogger<ReportCardController> logger, IControllerUtils controllerUtils) {
            _logger = logger;
            _controllerUtils = controllerUtils;
        }

        // GET: api/<ReportCardController>
        [HttpGet]
        public Response Get() {
            // Retrieves the username, password, and hacLink values from the URL Request.
            // Then logs in and fetches the result
            var result = _controllerUtils.GetResponse(HttpContext, _logger);

            return result;
        }
    }
}