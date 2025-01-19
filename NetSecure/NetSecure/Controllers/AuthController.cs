using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using ServiceContracts.DTO;

namespace NetSecure.Controllers
{
	[Route("auth")]
	public class AuthController : Controller
	{
		private readonly IUsersService _usersService;
		public AuthController(IUsersService usersService)
		{
			_usersService = usersService;
		}

		[HttpGet("login")]
		public IActionResult Login()
		{
			return View();
		}

		[HttpPost("login")]
		public IActionResult Login(LoginRequest loginRequest)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
					return View(loginRequest);
				}

				UserResponse? userResponse = _usersService.ValidateUser(loginRequest);
				if (userResponse == null)
				{
					ModelState.AddModelError(string.Empty, "Invalid username or password.");
					return View(loginRequest);
				}

				// Set user session or authentication token here
				return RedirectToAction("Index", "Home"); // Redirect to dashboard or another page
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(string.Empty, ex.Message);
				return View(loginRequest);
			}
		}
	}
}
