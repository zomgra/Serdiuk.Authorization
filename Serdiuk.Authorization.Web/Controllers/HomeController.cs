using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serdiuk.Authorization.Web.Infrastructure;
using System.Security.Claims;

namespace Serdiuk.Authorization.Web.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]/[action]")]
    public class HomeController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            var r = new Random().Next(0, 1000);
            return Ok(r);
        }
        [HttpGet]
        public IActionResult GetIdentity()
        {
            var userId = _userManager.GetUserId(User);
            var r = new{ Email=User.FindFirst(ClaimTypes.Email).Value, Id=userId };
            return Ok(r);
        }
    }
}
