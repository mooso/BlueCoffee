using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Nest;
using StretchyCloudData;
using System.Text.RegularExpressions;

namespace Indexer
{
	public class WorkerRole : RoleEntryPoint
	{
		private ElasticClient _client;
		private readonly static Regex LinkRegEx = new Regex("<a href=\"(?<Address>[^\"]*)\"",
			RegexOptions.Compiled);

		public override void Run()
		{
			while (true)
			{
				var toProbe = new Queue<string>();
				toProbe.Enqueue("http://en.wikipedia.org/wiki/Main_Page");
				using (var webClient = new WebClient())
				{
					while (toProbe.Count > 0)
					{
						var currentAddress = toProbe.Dequeue();
						Trace.TraceInformation("Probing: " + currentAddress);
						try
						{
							var pageContent = webClient.DownloadString(currentAddress);
							_client.Index(new WebPageInfo()
								{
									Address = currentAddress,
									Content = pageContent
								});
							var allLinks = LinkRegEx.Matches(pageContent);
							int added = 0;
							foreach (var link in allLinks.Cast<Match>().Select(l => l.Groups["Address"].Value))
							{
								var fixedLink = link;
								if (fixedLink.StartsWith("//"))
								{
									fixedLink = "http:" + link;
								}
								if (!fixedLink.StartsWith("http:"))
								{
									continue;
								}
								if (!_client.Search<WebPageInfo>(s => s.Query(a => a.Term(p => p.Address, fixedLink))).Documents.Any())
								{
									toProbe.Enqueue(fixedLink);
									added++;
								}
							}
							Trace.TraceInformation("Added " + added + " new links to probe.");
						}
						catch (Exception ex)
						{
							Trace.TraceWarning("Failed to probe: " + currentAddress + ". Exception: " + ex);
						}
						Thread.Sleep(100);
					}
				}
			}
		}

		public override bool OnStart()
		{
			var esRole = RoleEnvironment.Roles["ElasticSearch"];
			var esHosts = esRole.Instances.Select(i => i.InstanceEndpoints.First().Value.IPEndpoint.Address).ToList();
			var chosenHost = esHosts[new Random().Next(esHosts.Count)];
			var hostUri = new Uri("http://" + chosenHost + ":9200");
			Trace.TraceInformation("Connecting to " + hostUri);
			var settings = new ConnectionSettings(hostUri)
				.SetDefaultIndex("wikipedia");
			_client = new ElasticClient(settings);
			return base.OnStart();
		}
	}
}
