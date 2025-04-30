using Entities;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace NetSecure.Controllers
{
	[Route("GNS3")]
	public class GNS3Controller : Controller
	{
		private readonly UserDbContext _userDbContext;
		public GNS3Controller(UserDbContext userDbContext)
		{
			_userDbContext = userDbContext;
		}

		[HttpGet("Lab")]
		public IActionResult GNS3()
		{
			string? IP = HttpContext.Session.GetString("IP");
			ViewBag.IP = IP;
			var username = GetCurrentUsername();
			var user = _userDbContext.Users.FirstOrDefault(u => u.Username == username);
			ViewBag.Lab = user.SelectedLab;
			ViewBag.LabNumber = user.SelectedLab[user.SelectedLab.Length-1];
			if (user.SelectedLab.Equals("Custom"))
			{
				return RedirectToAction("Custom", "GNS3");
			}
			if (user.SelectedLab[0].Equals('A'))
			{
				ViewBag.level = "Advanced";
			}
			else if (user.SelectedLab[0].Equals('I'))
			{
				ViewBag.level = "Intermediate";
			}
			else if (user.SelectedLab[0].Equals('B'))
			{
				ViewBag.level = "Beginner";
			}
			return View();
		}

		[HttpGet("Custom")]
		public IActionResult Custom()
		{
			string? IP = HttpContext.Session.GetString("IP");
			ViewBag.IP = IP;
			var username = GetCurrentUsername();
			var user = _userDbContext.Users.FirstOrDefault(u => u.Username == username);
			ViewBag.Lab = user.SelectedLab;
			return View();
		}


		private string GetCurrentUsername()
		{
			return HttpContext.Session.GetString("Username");
		}
	}
}
