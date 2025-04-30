using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetSecure.Models;
using ServiceContracts;
using ServiceContracts.DTO;

namespace NetSecure.Controllers
{
	[Route("home")]
	public class HomeController : Controller
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly UserDbContext _userDbContext; 

		public HomeController(IHttpClientFactory httpClientFactory, UserDbContext userDbContext)
		{
			_httpClientFactory = httpClientFactory;
			_userDbContext = userDbContext;
		}
		[HttpGet("Index")]
		public async Task<IActionResult> Index()
		{
			string? IP = HttpContext.Session.GetString("IP"); // Retrieve IP
			if (string.IsNullOrEmpty(IP))
			{
				return BadRequest("VM IP not set.");
			}

			using HttpClient client = _httpClientFactory.CreateClient();
			string apiUrl = $"http://{IP}:80/static/web-ui/server/1/projects";

			try
			{
				string response = await client.GetStringAsync(apiUrl);
				ViewBag.IP = IP;
				return View();
			}
			catch (HttpRequestException ex)
			{
				return BadRequest($"Error connecting to GNS3: {ex.Message}");
			}
		}

		[HttpPost("SetIP")]
		public IActionResult SetIP([FromBody] IPModel model)
		{
			if (string.IsNullOrEmpty(model.IP))
			{
				return BadRequest(new { success = false, message = "Invalid IP" });
			}

			HttpContext.Session.SetString("IP", model.IP); // Store IP in Session

			return Ok(new { success = true });
		}

		[HttpGet("Levels")]
		public IActionResult Levels()
		{
			return View();
		}

		//[HttpPost("ResetProgress")]
		public async Task<IActionResult> ResetProgress()
		{
			try
			{
				var username = HttpContext.Session.GetString("Username");
				var user = _userDbContext.Users.FirstOrDefault(u => u.Username == username);

				if (user != null)
				{
					user.LabProgress = null;
					await _userDbContext.SaveChangesAsync();
					TempData["ResetMessage"] = "Your lab progress has been reset.";
				}
			}
			catch (Exception ex)
			{
				TempData["ResetMessage"] = $"Error resetting progress: {ex.Message}";
			}

			return RedirectToAction("Levels");
		}


		[HttpGet("Custom")]
		public IActionResult Custom()
		{
			var username = GetCurrentUsername();
			var user = _userDbContext.Users.FirstOrDefault(u => u.Username == username);
			user.SelectedLab = "Custom";
			_userDbContext.SaveChanges();
			return RedirectToAction("Custom","GNS3");
		}

		[HttpGet("Beginner/selectlab")]
		public IActionResult Beginner()
		{
			var username = HttpContext.Session.GetString("Username");
			var user = _userDbContext.Users.FirstOrDefault(u => u.Username == username);
			var finishedLabs = string.IsNullOrEmpty(user.LabProgress)
				? new List<string>()
				: user.LabProgress.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList();
			ViewBag.LabProgress = finishedLabs;
			return View();
		}

		[HttpPost("Beginner/selectlab")]
		public IActionResult Beginner(string lab)
		{
			if (string.IsNullOrEmpty(lab)) { return RedirectToAction("Levels", "Home"); }
			var username = GetCurrentUsername();
			var user = _userDbContext.Users.FirstOrDefault(u => u.Username == username);
			user.SelectedLab = lab;
			_userDbContext.SaveChanges();
			return RedirectToAction("LoadLabB", "Lab", new { labName = lab });

		}

		[HttpGet("Intermediate/selectlab")]
		public IActionResult Intermediate()
		{
			var username = HttpContext.Session.GetString("Username");
			var user = _userDbContext.Users.FirstOrDefault(u => u.Username == username);
			var finishedLabs = string.IsNullOrEmpty(user.LabProgress)
				? new List<string>()
				: user.LabProgress.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList();
			ViewBag.LabProgress = finishedLabs;
			return View();
		}

		[HttpPost("Intermediate/selectlab")]
		public IActionResult Intermediate(string lab)
		{
			if (string.IsNullOrEmpty(lab)) { return RedirectToAction("Levels", "Home"); }
			var username = GetCurrentUsername();
			var user = _userDbContext.Users.FirstOrDefault(u => u.Username == username);
			user.SelectedLab = lab;
			_userDbContext.SaveChanges();
			return RedirectToAction("LoadLabI", "Lab", new { labName = lab });
		}

		[HttpGet("Advanced/selectlab")]
		public IActionResult Advanced()
		{
			var username = HttpContext.Session.GetString("Username");
			var user = _userDbContext.Users.FirstOrDefault(u => u.Username == username);
			var finishedLabs = string.IsNullOrEmpty(user.LabProgress)
				? new List<string>()
				: user.LabProgress.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList();
			ViewBag.LabProgress = finishedLabs;
			return View();
		}

		[HttpPost("Advanced/selectlab")]
		public IActionResult Advanced(string lab)
		{
			if (string.IsNullOrEmpty(lab)) { return RedirectToAction("Levels", "Home"); }
			var username = GetCurrentUsername();
			var user = _userDbContext.Users.FirstOrDefault(u => u.Username == username);
			user.SelectedLab = lab;
			_userDbContext.SaveChanges();
			return RedirectToAction("LoadLabA", "Lab", new { labName = lab });
		}

		private string GetCurrentUsername()
		{
			return HttpContext.Session.GetString("Username");
		}
	}
}
