using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RecipeBookApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("API работи успешно!");
        }

        // Добавяме маршрут за кореновия път "/"
        [HttpGet("/")]
        public IActionResult MainPage()
        {
            return Ok("Добре дошъл в RecipeBook API!");
        }
    }
}