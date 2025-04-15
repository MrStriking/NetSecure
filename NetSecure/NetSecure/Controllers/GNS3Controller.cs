using Microsoft.AspNetCore.Mvc;

namespace NetSecure.Controllers
{
	[Route("GNS3")]
	public class GNS3Controller : Controller
	{
		[HttpGet("Lab")]
		public IActionResult GNS3()
		{
			string? IP = HttpContext.Session.GetString("IP");
			ViewBag.IP = IP;
			return View();
		}
	}
}
