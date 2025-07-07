using Microsoft.AspNetCore.Mvc;

namespace AltecSystem.Api.Controllers
{
    [ApiController]
    [Route("/")]
    public class RootController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("🚀 API ALTEC desplegada con éxito.");
        }
    }
}