using Entities;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using ServiceContracts.DTO;

namespace NetSecure.Controllers
{
	[Route("auth")]
	public class AuthController : Controller
	{
		private readonly IUsersService _usersService;
		private readonly UserDbContext _userDbContext;

		public AuthController(IUsersService usersService, UserDbContext userDbContext)
		{
			_usersService = usersService;
			_userDbContext = userDbContext;
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
				HttpContext.Session.SetString("Username", userResponse.Username);
				var username = GetCurrentUsername();
				if (username == "Admin")
				{
					return RedirectToAction("Index", "Admin");
				}
				if (string.IsNullOrEmpty(username)) return Unauthorized();
				var user = _userDbContext.Users.FirstOrDefault(u => u.Username == username);
				if (user.SelectedLevel == null)
				{
					return RedirectToAction("Levels", "Home");
				}
				return RedirectToAction("Index", "Home");
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(string.Empty, ex.Message);
				return View(loginRequest);
			}
		}

		private string GetCurrentUsername()
		{
			return HttpContext.Session.GetString("Username");
		}
	}
}
