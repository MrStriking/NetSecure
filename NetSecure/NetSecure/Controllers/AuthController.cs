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
				var user = _userDbContext.Users.FirstOrDefault(u => u.Username == userResponse.Username);
				if (user.IsAdmin)
				{
					return RedirectToAction("Index", "Admin");
				}


				if (string.IsNullOrEmpty(user.SelectedLab))
				{
					return RedirectToAction("Levels", "Home");
				}
				else if (user.SelectedLab[0].Equals('A'))
				{
					return RedirectToAction("LoadLabA", "Lab", new { labName = user.SelectedLab });
				}
				else if (user.SelectedLab[0].Equals('I'))
				{
					return RedirectToAction("LoadLabI", "Lab", new { labName = user.SelectedLab });
				}
				else if (user.SelectedLab[0].Equals('B'))
				{
					return RedirectToAction("LoadLabB", "Lab", new { labName = user.SelectedLab });
				}
				else if (user.SelectedLab[0].Equals('C'))
				{
					return RedirectToAction("Custom", "GNS3");
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
