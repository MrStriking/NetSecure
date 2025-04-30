using Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NetSecure.Controllers
{
	//[ApiController]
	[Route("Evaluation")]
	public class EvaluationController : Controller
	{
		private HttpClient _client;
		private readonly UserDbContext _userDbContext;
		private readonly IHttpClientFactory _httpClientFactory;
		private string? IP;
		private string LabName;

		public EvaluationController(IHttpClientFactory httpClientFactory, UserDbContext userDbContext)
		{
			_httpClientFactory = httpClientFactory;
			_userDbContext = userDbContext;
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			base.OnActionExecuting(context);

			IP = HttpContext.Session.GetString("IP");
			IP = $"http://{IP}/v2/projects";
			var username = HttpContext.Session.GetString("Username");

			if (!string.IsNullOrEmpty(IP))
			{
				_client = _httpClientFactory.CreateClient();
				_client.BaseAddress = new Uri(IP);
			}

			if (!string.IsNullOrEmpty(username))
			{
				var user = _userDbContext.Users.FirstOrDefault(u => u.Username == username);
				if (user != null)
				{
					LabName = user.SelectedLab;	
				}
			}
		}
		private void Progress()
		{
			var username = HttpContext.Session.GetString("Username");
			var user = _userDbContext.Users.FirstOrDefault(u => u.Username == username);

			if (user != null)
			{
				var finishedLabs = string.IsNullOrEmpty(user.LabProgress)
					? new List<string>()
					: user.LabProgress.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList();

				if (!finishedLabs.Contains(user.SelectedLab))
					finishedLabs.Add(user.SelectedLab);

				user.LabProgress = string.Join(';', finishedLabs);

				_userDbContext.SaveChanges();
			}
		}
		[HttpGet("evaluate-blab1")]
		public async Task<IActionResult> BLab1()
		{
			try
			{
				var project = await GetProjectByName(LabName);
				if (project == null)
				{
					ViewBag.Success = false;
					ViewBag.Message = $"Project '{LabName}' not found";
					return View("BLab1");
				}

				var nodes = await GetNodes(project.project_id);
				if (nodes == null || !nodes.Any())
				{
					ViewBag.Success = false;
					ViewBag.Message = "No nodes found in the project";
					return View("BLab1");
				}

				var pc1 = nodes.FirstOrDefault(n => n.name.Equals("PC1", StringComparison.OrdinalIgnoreCase));
				var pc2 = nodes.FirstOrDefault(n => n.name.Equals("PC2", StringComparison.OrdinalIgnoreCase));
				var switches = nodes.Where(n => n.name.Contains("switch", StringComparison.OrdinalIgnoreCase)).ToList();

				if (pc1 == null || pc2 == null || switches.Count != 1 || !LabName.Equals("BLab1"))
				{
					ViewBag.Success = false;
					ViewBag.Message = "Missing or incorrect components.";
					ViewBag.MissingComponents = new List<string>();
					if (pc1 == null) ViewBag.MissingComponents.Add("PC1");
					if (pc2 == null) ViewBag.MissingComponents.Add("PC2");
					if (switches.Count != 1) ViewBag.MissingComponents.Add($"Switch (found: {switches.Count})");
					if (!LabName.Equals("BLab1")) ViewBag.MissingComponents.Add();
					return View("BLab1");
				}

				ViewBag.Success = true;
				ViewBag.Message = "All required components found!";
				ViewBag.PC1 = pc1.name;
				ViewBag.PC2 = pc2.name;
				ViewBag.SwitchCount = switches.Count;
				ViewBag.LabName = LabName;
				Progress();
				return View("BLab1");
			}
			catch (Exception ex)
			{
				ViewBag.Success = false;
				ViewBag.Message = "An error occurred: " + ex.Message;
				return View("BLab1");
			}
		}


		[HttpGet("evaluate-blab2")]
		public async Task<IActionResult> BLab2()
		{
			try
			{
				// 1. Check if lab1 project exists
				var project = await GetProjectByName(LabName);
				if (project == null)
					return BadRequest(new { error = $"Project '{LabName}' not found" });

				// 2. Get all nodes in the project
				var nodes = await GetNodes(project.project_id);
				if (nodes == null || !nodes.Any())
					return BadRequest(new { error = "No nodes found in the project" });

				// 3. Find required nodes
				var pc1 = nodes.FirstOrDefault(n => n.name.Equals("PC1", StringComparison.OrdinalIgnoreCase));
				var pc2 = nodes.FirstOrDefault(n => n.name.Equals("PC2", StringComparison.OrdinalIgnoreCase));
				var pc3 = nodes.FirstOrDefault(n => n.name.Equals("PC3", StringComparison.OrdinalIgnoreCase));
				var pc4 = nodes.FirstOrDefault(n => n.name.Equals("PC4", StringComparison.OrdinalIgnoreCase));
				var switches = nodes.Where(n => n.name.Contains("switch", StringComparison.OrdinalIgnoreCase)).ToList();
				var router = nodes.Where(n => n.name.Contains("router", StringComparison.OrdinalIgnoreCase)).ToList();

				// 4. Verify components
				if (pc1 == null || pc2 == null || pc3 == null || pc4==null || switches.Count != 1 || router.Count!=1)
				{
					var missing = new List<string>();
					if (pc1 == null) missing.Add("PC1");
					if (pc2 == null) missing.Add("PC2");
					if (pc3 == null) missing.Add("PC3");
					if (pc4 == null) missing.Add("PC4");
					if (switches.Count != 1) missing.Add($"Switch (found: {switches.Count})");
					if (router.Count != 1) missing.Add($"Router (found: {router.Count})");

					return BadRequest(new
					{
						success = false,
						error = "Missing/incorrect components",
						details = missing
					});
				}

				// 5. Success response
				return Ok(new
				{
					success = true,
					message = "All required components found",
					components = new
					{
						pc1 = pc1.name,
						pc2 = pc2.name,
						pc3 = pc3.name,
						pc4 = pc4.name,
						switchCount = switches.Count,
						routerCount = router.Count
					}
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { error = ex.Message });
			}
		}

		[HttpGet("evaluate-Ilab1")]
		public async Task<IActionResult> ILab1()
		{
			try
			{
				var project = await GetProjectByName(LabName);
				if (project == null)
				{
					ViewBag.Success = false;
					ViewBag.Message = $"Project '{LabName}' not found";
					return View("BLab1");
				}

				var nodes = await GetNodes(project.project_id);
				if (nodes == null || !nodes.Any())
				{
					ViewBag.Success = false;
					ViewBag.Message = "No nodes found in the project";
					return View("ILab1");
				}

				var pc1 = nodes.FirstOrDefault(n => n.name.Equals("PC1", StringComparison.OrdinalIgnoreCase));
				var nat1 = nodes.FirstOrDefault(n => n.name.Equals("NAT1", StringComparison.OrdinalIgnoreCase));

				if (pc1 == null || nat1 == null || !LabName.Equals("ILab1"))
				{
					ViewBag.Success = false;
					ViewBag.Message = "Missing or incorrect components.";
					ViewBag.MissingComponents = new List<string>();
					if (pc1 == null) ViewBag.MissingComponents.Add("PC1");
					if (nat1 == null) ViewBag.MissingComponents.Add("NAT1");
					if (!LabName.Equals("ILab1")) ViewBag.MissingComponents.Add();
					return View("ILab1");
				}

				ViewBag.Success = true;
				ViewBag.Message = "All required components found! 🎉";
				ViewBag.PC1 = pc1.name;
				ViewBag.NAT1 = nat1.name;
				ViewBag.LabName = LabName;
				Progress();
				return View("ILab1");
			}
			catch (Exception ex)
			{
				ViewBag.Success = false;
				ViewBag.Message = "An error occurred: " + ex.Message;
				return View("ILab1");
			}
		}

		private async Task<Project> GetProjectByName(string projectName)
		{
			var response = await _client.GetAsync("projects");
			if (!response.IsSuccessStatusCode)
			{
				throw new Exception($"Failed to fetch projects. Status code: {response.StatusCode}");
			}
			var content = await response.Content.ReadAsStringAsync();
			try
			{
				var projects = JsonSerializer.Deserialize<List<Project>>(content);
				return projects?.FirstOrDefault(p => p.name.Equals(projectName, StringComparison.OrdinalIgnoreCase));
			}
			catch (Exception ex)
			{
				throw new Exception($"Failed to deserialize project list: {ex.Message}");
			}
		}

		private async Task<List<Node>> GetNodes(string projectId)
		{
			var response = await _client.GetAsync($"projects/{projectId}/nodes");
			var content = await response.Content.ReadAsStringAsync();
			return JsonSerializer.Deserialize<List<Node>>(content);
		}

		private string GetCurrentUsername()
		{
			return HttpContext.Session.GetString("Username");
		}

		#region Model Classes
		class Project
		{
			public string project_id { get; set; }
			public string name { get; set; }
		}

		class Node
		{
			public string node_id { get; set; }
			public string name { get; set; }
		}
		#endregion
	}
}