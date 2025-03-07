using Microsoft.AspNetCore.Mvc;

namespace RecipeBookApi.Controllers
{
    [Route("api")]
    [ApiController]
    public class RootController
    {
        [HttpGet("")]
        public IActionResult GetApiStatus()
        {
            return new OkObjectResult("Greetings API is working"); // ✅ Работи без Ok()
        }
    }
}
