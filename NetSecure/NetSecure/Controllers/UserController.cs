using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using ServiceContracts.DTO;

namespace NetSecure.Controllers
{
	[Route("Sign-up")]
	public class UserController : Controller
	{
		private readonly IUsersService _usersService;
		public UserController(IUsersService usersService)
		{
			_usersService = usersService;
		}

		[HttpGet]
		public IActionResult Adduser()
		{
			return View();
		}

		[HttpPost]
		public IActionResult AddUser(UserAddRequest userAddRequest)
		{
			if (!ModelState.IsValid)
			{
				ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
				return View();
			}
			UserResponse userResponse = _usersService.AddUser(userAddRequest);
			return View();
		}
	}
}
