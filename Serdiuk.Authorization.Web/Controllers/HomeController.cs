using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Serdiuk.Authorization.Web.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]/[action]")]
    public class HomeController : Controller
    {
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            var r = new Random().Next(0, 1000);
            return Ok(r);
        }
        [HttpGet]
        public IActionResult IdentityRead()
        {

            var r = new Random().Next(10000, 1000000000);
            return Ok(r);
        }
    }
}
