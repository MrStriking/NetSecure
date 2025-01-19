using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using ServiceContracts.DTO;


namespace NetSecure.Controllers
{
	[Route("index")]
	[Route("/")]
	public class IndexController : Controller
	{
		
		public IActionResult Index()
		{
			return View();
		}
	}
}
