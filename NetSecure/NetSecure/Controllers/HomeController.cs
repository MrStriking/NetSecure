using Microsoft.AspNetCore.Mvc;

namespace NetSecure.Controllers
{
	[Route("home")]
	public class HomeController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
