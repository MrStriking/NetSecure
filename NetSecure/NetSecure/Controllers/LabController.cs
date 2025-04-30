using Entities;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace NetSecure.Controllers
{
	[Route("Lab")]
	public class LabController : Controller
	{
		[HttpGet("Beginner/{labName}")]
		public IActionResult LoadLabB(string labName)
		{
			return View($"~/Views/Lab/Beginner/{labName}/{labName}.cshtml");
		}

		[HttpGet("Intermediate/{labName}")]
		public IActionResult LoadLabI(string labName)
		{
			return View($"~/Views/Lab/Intermediate/{labName}/{labName}.cshtml");
		}

		[HttpGet("Advanced/{labName}")]
		public IActionResult LoadLabA(string labName)
		{
			return View($"~/Views/Lab/Advanced/{labName}/{labName}.cshtml");
		}

		[HttpGet("LoadSetup")]
		public IActionResult LoadSetup(string labname)
		{
			string setup = labname + "Setup";
			string level="";
			if (labname[0].Equals('B')) { level = "Beginner"; }
			else if (labname[0].Equals('I')) { level = "Intermediate"; }
			else if (labname[0].Equals('A')) { level = "Advanced"; }
			return View($"~/Views/Lab/{level}/{labname}/{setup}.cshtml");
		}

		public IActionResult LoadGNS3()
		{
			return RedirectToAction("GNS3", "GNS3");
		}
	}
}
