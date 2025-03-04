using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using ServiceContracts.DTO;

namespace NetSecure.Controllers
{
	[Route("sign-up")]
	public class UserController : Controller
	{
		private readonly IUsersService _usersService;
		public UserController(IUsersService usersService)
		{
			_usersService = usersService;
		}

		[HttpGet]
		public IActionResult SignUp()
		{
			return View();
		}

		[HttpPost]
		public IActionResult SignUp(UserAddRequest userAddRequest)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
					return View(userAddRequest);
				}
				UserResponse userResponse = _usersService.AddUser(userAddRequest);
				return RedirectToAction("Login","Auth");
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(string.Empty, ex.Message);
				return View(userAddRequest);
			}
		}
	}
}
