using Microsoft.AspNetCore.Mvc;
using NetSecure.Models;

namespace NetSecure.Controllers
{
	[Route("home")]
	public class HomeController : Controller
	{
		private readonly IHttpClientFactory _httpClientFactory;

		public HomeController(IHttpClientFactory httpClientFactory)
		{
			_httpClientFactory = httpClientFactory;
		}

		public async Task<IActionResult> Index()
		{
			string? IP = HttpContext.Session.GetString("IP"); // Retrieve stored VM IP
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

		

	}
}
