using HAC.API.Data.Objects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HAC.API.Controllers {
    [Route("api/student")]
    [ApiController]
    public class StudentController : ControllerBase {
        private readonly IControllerUtils _controllerUtils;
        private readonly ILogger<StudentController> _logger;

        public StudentController(ILogger<StudentController> logger, IControllerUtils controllerUtils) {
            _logger = logger;
            _controllerUtils = controllerUtils;
        }

        // GET: api/<StudentController>
        [HttpGet]
        public Response Get() {
            // Retrieves the username, password, and hacLink values from the URL Request.
            // Then logs in and fetches the result
            var result = _controllerUtils.GetResponse(HttpContext, _logger);

            return result;
        }
    }
}