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

		[HttpPost("Levels")]
		public async Task<IActionResult> Levels([FromBody] Dictionary<string, string> data)
		{
			if (!data.TryGetValue("level", out var level) || string.IsNullOrEmpty(level))
			{
				return BadRequest("Invalid level");
			}
			var username = GetCurrentUsername();
			if (string.IsNullOrEmpty(username)) return Unauthorized();
			var user = await _userDbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
			user.SelectedLevel = level;
			await _userDbContext.SaveChangesAsync();
			return RedirectToAction("Index","Home");
		}

		private string GetCurrentUsername()
		{
			return HttpContext.Session.GetString("Username");
		}
	}
}
