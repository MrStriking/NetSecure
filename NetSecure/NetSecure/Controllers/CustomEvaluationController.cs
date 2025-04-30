using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace NetSecure.Controllers
{
	[Route("CustomEvaluation")]
	public class CustomEvaluationController : Controller
	{
		private readonly HttpClient _httpClient;
		private readonly UserDbContext _userDbContext;
		private string? IP;
		private string LabName;

		public CustomEvaluationController(IHttpClientFactory httpClientFactory, UserDbContext userDbContext)
		{
			_httpClient = httpClientFactory.CreateClient();
			_userDbContext = userDbContext;
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			base.OnActionExecuting(context);

			IP = HttpContext.Session.GetString("IP");
			IP = $"http://{IP}/v2/projects";
			var username = HttpContext.Session.GetString("Username");

			if (!string.IsNullOrEmpty(username))
			{
				var user = _userDbContext.Users.FirstOrDefault(u => u.Username == username);
				if (user != null)
				{
					if (user.SelectedLab.Equals("Custom"))
					{
						LabName = user.SelectedLab;
					}
				}
			}
		}

		[HttpGet("Report")]
		public async Task<IActionResult> EvaluateLab([FromQuery] string projectName, [FromQuery] string infectedNode = null)
		{
			if (string.IsNullOrWhiteSpace(projectName))
				return BadRequest("Project name is required.");

			string gns3ApiBase = IP;

			try
			{
				// Step 1: Get all projects
				var projectResponse = await _httpClient.GetAsync(gns3ApiBase);
				projectResponse.EnsureSuccessStatusCode();

				var projectContent = await projectResponse.Content.ReadAsStringAsync();
				var projects = JArray.Parse(projectContent);

				var matchingProject = projects
					.FirstOrDefault(p => string.Equals((string)p["name"], projectName, System.StringComparison.OrdinalIgnoreCase));

				if (matchingProject == null)
					return NotFound(new { message = "❌ Project not found. Please check the name and try again." });

				string projectId = (string)matchingProject["project_id"];

				// Step 2: Fetch nodes
				var nodesResponse = await _httpClient.GetAsync($"{gns3ApiBase}/{projectId}/nodes");
				nodesResponse.EnsureSuccessStatusCode();
				var nodesContent = await nodesResponse.Content.ReadAsStringAsync();
				var nodes = JArray.Parse(nodesContent);

				var deviceCounts = new Dictionary<string, int>();
				var nodeList = new List<dynamic>();
				var nodeIdToName = new Dictionary<string, string>();
				var deviceTypesUsed = new HashSet<string>();
				bool firewallPresent = false;

				foreach (var node in nodes)
				{
					string nodeId = node["node_id"]?.ToString();
					string name = node["name"]?.ToString();
					string type = node["node_type"]?.ToString() ?? "Unknown";

					nodeIdToName[nodeId] = name;

					if (!deviceCounts.ContainsKey(type))
						deviceCounts[type] = 0;
					deviceCounts[type]++;

					deviceTypesUsed.Add(type);

					if (type.ToLower().Contains("firewall") || name.ToLower().Contains("firewall"))
						firewallPresent = true;

					var nodeDetailResponse = await _httpClient.GetAsync($"{gns3ApiBase}/{projectId}/nodes/{nodeId}");
					nodeDetailResponse.EnsureSuccessStatusCode();
					var nodeDetailContent = await nodeDetailResponse.Content.ReadAsStringAsync();
					var nodeDetail = JObject.Parse(nodeDetailContent);

					var ports = nodeDetail["ports"] as JArray ?? new JArray();
					var interfaces = ports.Select(p => new
					{
						name = (string)p["name"],
						shortName = (string)p["short_name"],
						adapter = (int?)p["adapter_number"],
						port = (int?)p["port_number"],
						linkType = (string)p["link_type"]
					}).ToList();

					nodeList.Add(new { name, type, interfaces, nodeId });
				}

				// Step 3: Fetch links
				var linksResponse = await _httpClient.GetAsync($"{gns3ApiBase}/{projectId}/links");
				linksResponse.EnsureSuccessStatusCode();
				var linksContent = await linksResponse.Content.ReadAsStringAsync();
				var links = JArray.Parse(linksContent);

				var totalDevices = nodes.Count;
				var totalLinks = links.Count;

				// Network Evaluation: Find connected nodes
				var connectedNodeIds = new HashSet<string>();
				foreach (var link in links)
				{
					foreach (var n in link["nodes"])
					{
						string nid = n["node_id"]?.ToString();
						if (nid != null) connectedNodeIds.Add(nid);
					}
				}

				var networkFeedback = new List<string>();
				foreach (var node in nodeList)
				{
					string nodeId = node.nodeId;
					string name = node.name;
					if (!connectedNodeIds.Contains(nodeId))
						networkFeedback.Add($"❌ {name} is isolated and not connected to any network.");
				}

				if (networkFeedback.Count == 0)
					networkFeedback.Add("✔ All devices are connected.");

				// Device Diversity
				int deviceDiversityScore = deviceTypesUsed.Count;
				string diversityFeedback = deviceDiversityScore < 2
					? "🧠 Consider adding a router or switch to enable communication."
					: "👍 Nice variety of devices used.";

				// Firewall Feedback
				string firewallFeedback = firewallPresent
					? "✅ Nice! You've included a firewall in your topology."
					: "⚠️ Consider adding a firewall for better protection.";

				// Segmentation Feedback (basic logic)
				var switchAndRouterCount = nodeList.Count(n =>
					n.type.ToLower().Contains("switch") || n.type.ToLower().Contains("router"));

				string segmentationLevel = switchAndRouterCount >= 2 ? "partial" : "none";
				string segmentationFeedback = switchAndRouterCount >= 2
					? "🔐 You've started segmenting the network. Consider adding a router to fully separate traffic."
					: "🔄 All devices are in a single segment. Consider splitting them for better security.";

				// Simulated Port Scan Logic
				var portScanFeedback = await SimulatePortScan(projectId, nodeList, links);

				//Simulated Malware Propagation
				var malwarePropagationFeedback = await SimulateMalwarePropagation(projectId, nodeList, links, infectedNode);

				// Single Point of Failure
				var spofFeedback = await SimulateSinglePointsOfFailure(nodeList, links);

				// Top-level info
				string finalProjectId = projectId;
				int finalTotalDevices = totalDevices;
				int finalTotalLinks = totalLinks;
				var finalDeviceCounts = deviceCounts;
				var finalNodeList = nodeList;

				// Evaluation details
				var finalNetworkFeedback = networkFeedback;
				int finalDiversityScore = deviceDiversityScore;
				string finalDiversityFeedback = diversityFeedback;
				bool finalFirewallPresent = firewallPresent;
				string finalFirewallFeedback = firewallFeedback;
				string finalSegmentationLevel = segmentationLevel;
				string finalSegmentationFeedback = segmentationFeedback;

				// Simulations
				var finalPortScanFeedback = portScanFeedback.securityFeedback;
				var finalMalwarePropagationFeedback = malwarePropagationFeedback;
				var finalSpofFeedback = spofFeedback.spofFeedback;


				return View("Report", new
				{
					projectId = finalProjectId,
					totalDevices = finalTotalDevices,
					deviceCounts = finalDeviceCounts,
					totalLinks = finalTotalLinks,
					nodes = finalNodeList.Select(n => new
					{
						n.name,
						n.type,
						n.interfaces
					}),
					evaluation = new
					{
						networkFeedback = finalNetworkFeedback,
						deviceDiversityScore = finalDiversityScore,
						diversityFeedback = finalDiversityFeedback,
						firewallPresent = finalFirewallPresent,
						firewallFeedback = finalFirewallFeedback,
						networkSegmentation = finalSegmentationLevel,
						segmentationFeedback = finalSegmentationFeedback,
						portScanFeedback = finalPortScanFeedback,
						malwarePropagationFeedback = finalMalwarePropagationFeedback,
						spofFeedback = finalSpofFeedback
					}
				});

			}
			catch (HttpRequestException ex)
			{
				return StatusCode(500, $"Error connecting to GNS3 API: {ex.Message}");
			}
		}






		// Simulated Port Scan Logic
		private async Task<dynamic> SimulatePortScan(string projectId, List<dynamic> nodeList, JArray links)
		{
			var scanResults = new List<dynamic>();

			// Include vpcs, router, and switch node types for scanning
			var scanTargetTypes = new[] { "vpcs", "router", "switch" };

			// Filter all nodes that match the scanTargetTypes
			var sourceNodes = nodeList
				.Where(n => scanTargetTypes.Any(t => n.type.ToLower().Contains(t)))
				.ToList();

			if (!sourceNodes.Any())
			{
				return new
				{
					securityFeedback = new[] { new { message = "❌ No nodes found for port scan simulation (vpcs/router/switch).", passed = false } }
				};
			}

			foreach (var sourceNode in sourceNodes)
			{
				var reachableNodes = new HashSet<string>();
				var visited = new HashSet<string>();
				var queue = new Queue<string>();

				queue.Enqueue(sourceNode.nodeId);
				visited.Add(sourceNode.nodeId);

				while (queue.Count > 0)
				{
					var currentNodeId = queue.Dequeue();
					reachableNodes.Add(currentNodeId);

					var connectedLinks = links.Where(link =>
						link["nodes"].Any(n => n["node_id"]?.ToString() == currentNodeId));

					foreach (var link in connectedLinks)
					{
						foreach (var node in link["nodes"])
						{
							string nodeId = node["node_id"]?.ToString();
							if (nodeId != null && !visited.Contains(nodeId))
							{
								visited.Add(nodeId);
								queue.Enqueue(nodeId);
							}
						}
					}
				}

				bool allReachable = reachableNodes.Count == nodeList.Count;

				scanResults.Add(new
				{
					from = sourceNode.name,
					reachableCount = reachableNodes.Count,
					totalDevices = nodeList.Count,
					passed = !allReachable,
					message = allReachable
						? $"❌ {sourceNode.name} can reach all {nodeList.Count} devices. No segmentation."
						: $"✅ {sourceNode.name} can only reach {reachableNodes.Count}/{nodeList.Count} devices. Segmentation is working."
				});
			}

			return new { securityFeedback = scanResults };
		}


		private async Task<dynamic> SimulateMalwarePropagation(string projectId, List<dynamic> nodeList, JArray links, string infectedNodeName = null)
		{
			dynamic infectedNode;

			if (!string.IsNullOrWhiteSpace(infectedNodeName))
			{
				infectedNode = nodeList.FirstOrDefault(n =>
					string.Equals(n.name, infectedNodeName, StringComparison.OrdinalIgnoreCase));
			}
			else
			{
				infectedNode = nodeList.FirstOrDefault(); // fallback
			}

			if (infectedNode == null)
			{
				return new
				{
					message = "❌ No valid infected node found for simulation.",
					passed = false
				};
			}

			var visited = new HashSet<string>();
			var queue = new Queue<string>();
			queue.Enqueue(infectedNode.nodeId);
			visited.Add(infectedNode.nodeId);

			while (queue.Count > 0)
			{
				var current = queue.Dequeue();
				var connectedLinks = links.Where(link =>
					link["nodes"].Any(n => n["node_id"]?.ToString() == current));

				foreach (var link in connectedLinks)
				{
					foreach (var n in link["nodes"])
					{
						string neighborId = n["node_id"]?.ToString();
						if (neighborId != null && !visited.Contains(neighborId))
						{
							visited.Add(neighborId);
							queue.Enqueue(neighborId);
						}
					}
				}
			}

			bool infectedAll = visited.Count == nodeList.Count;

			return new
			{
				infectedNode = infectedNode.name,
				totalInfected = visited.Count,
				totalDevices = nodeList.Count,
				passed = !infectedAll,
				message = infectedAll
					? $"❌ Malware spread from {infectedNode.name} to all {nodeList.Count} devices. Improve segmentation."
					: $"✅ Malware from {infectedNode.name} only reached {visited.Count}/{nodeList.Count} devices. Segmentation is effective."
			};
		}


		private async Task<dynamic> SimulateSinglePointsOfFailure(List<dynamic> nodeList, JArray links)
		{
			var spofResults = new List<dynamic>();

			foreach (var node in nodeList)
			{
				var remainingNodes = nodeList.Where(n => n.nodeId != node.nodeId).ToList();

				// Build a modified links list without links related to the current node
				var modifiedLinks = new JArray(
					links.Where(link =>
						!link["nodes"].Any(n => n["node_id"]?.ToString() == node.nodeId))
				);

				// Perform connectivity check
				var visited = new HashSet<string>();
				var queue = new Queue<string>();

				if (remainingNodes.Any())
				{
					queue.Enqueue(remainingNodes[0].nodeId);
					visited.Add(remainingNodes[0].nodeId);
				}

				while (queue.Count > 0)
				{
					var current = queue.Dequeue();
					var connectedLinks = modifiedLinks.Where(link =>
						link["nodes"].Any(n => n["node_id"]?.ToString() == current));

					foreach (var link in connectedLinks)
					{
						foreach (var n in link["nodes"])
						{
							string neighborId = n["node_id"]?.ToString();
							if (neighborId != null && !visited.Contains(neighborId))
							{
								visited.Add(neighborId);
								queue.Enqueue(neighborId);
							}
						}
					}
				}

				bool causesDisconnection = visited.Count != remainingNodes.Count;

				spofResults.Add(new
				{
					removedNode = node.name,
					causesDisconnection,
					message = causesDisconnection
						? $"❌ Removing {node.name} disconnects part of the network. It is a Single Point of Failure! Add backup paths, devices, or connections. "
						: $"✅ Removing {node.name} does NOT disconnect the network. Good resilience."
				});
			}

			return new { spofFeedback = spofResults };
		}
	}
}
