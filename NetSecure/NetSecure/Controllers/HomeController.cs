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

		[HttpGet("SelectLab/beginner")]
		public IActionResult Beginner()
		{
			var username = GetCurrentUsername();
			var user =  _userDbContext.Users.FirstOrDefault(u => u.Username == username);
			user.SelectedLevel = "Beginner";
			_userDbContext.SaveChanges();
			return View();
		}

		[HttpGet("SelectLab/intermediate")]
		public IActionResult Intermediate()
		{
			var username = GetCurrentUsername();
			var user = _userDbContext.Users.FirstOrDefault(u => u.Username == username);
			user.SelectedLevel = "Intermediate";
			_userDbContext.SaveChanges();
			return View();
		}

		[HttpGet("SelectLab/advanced")]
		public IActionResult Advanced()
		{
			var username = GetCurrentUsername();
			var user = _userDbContext.Users.FirstOrDefault(u => u.Username == username);
			user.SelectedLevel = "Advanced";
			_userDbContext.SaveChanges();
			return View();
		}

		private string GetCurrentUsername()
		{
			return HttpContext.Session.GetString("Username");
		}
	}
}
