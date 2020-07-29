using Microsoft.AspNetCore.Mvc;
namespace BeatSlayerServer.Controllers
{
    public class ErrorController : Controller
    {
        [Route("error/404")]
        public IActionResult Error404()
        {
            return View();
        }

        [Route("error/{code:int}")]
        public IActionResult Error(int code)
        {
            // handle different codes or just return the default error view
            return Content("Status code is " + code);
        }
    }
}
