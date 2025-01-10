using Microsoft.AspNetCore.Mvc;

namespace NetSecure.Controllers
{
	public class UserController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
