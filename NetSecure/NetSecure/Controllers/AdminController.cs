using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO;

namespace NetSecure.Controllers
{
	[Route("Admin")]
	public class AdminController : Controller
	{
		private readonly IUsersService _usersService;

		public AdminController(IUsersService usersService)
		{
			_usersService = usersService;
		}
		public IActionResult Index()
		{
			int count = _usersService.GetUserCount();
			int adminCount = _usersService.GetAdminCount();
			ViewBag.UserCount = count;
			ViewBag.AdminCount = adminCount;
			return View();
		}

		[HttpPost("remove-user")]
		public IActionResult RemoveUser(String removeUsername)
		{
			bool x = _usersService.DeleteUser(removeUsername);
			if (!x)
			{
				ModelState.AddModelError(String.Empty, "Invalid Username");
			}
			return RedirectToAction("Index","Admin");
		}

		[HttpPost("make-admin")]
		public IActionResult MakeAdmin(String adminUsername)
		{
			bool x = _usersService.MakeAdmin(adminUsername);
			if (!x)
			{
				ModelState.AddModelError(String.Empty, "Error");
			}
			return RedirectToAction("Index", "Admin");
		}
	}
}
