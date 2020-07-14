using HAC.API.Data.Objects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HAC.API.Controllers {
    [Route("api/reportCard")]
    [ApiController]
    public class ReportCardController : ControllerBase {
        private readonly ILogger<ReportCardController> _logger;

        public ReportCardController(ILogger<ReportCardController> logger) {
            _logger = logger;
        }


        // GET: api/<ReportCardController>
        [HttpGet]
        public Response Get() {
            // Retrieves the username, password, and hacLink values from the URL Request.
            // Then logs in and fetches the result
            var result = Utils.GetResponse(HttpContext, _logger);

            return result;
        }
    }
}