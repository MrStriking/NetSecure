using Microsoft.AspNetCore.Mvc;

namespace NetSecure.Controllers
{
	[Route("Admin")]
	public class AdminController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
