using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Erpmi.Controllers
{
    [Authorize(Policy = "ApiUser")]
    [Route("api/[controller]")]
    public class ExamsController
    {
        // GET api/exams/get
        [HttpGet("get")]
        public IActionResult GetExams()
        {
            return new OkObjectResult(new { DisplayName = "Mark", CompanyName = "A company" });
        }
    }
}
