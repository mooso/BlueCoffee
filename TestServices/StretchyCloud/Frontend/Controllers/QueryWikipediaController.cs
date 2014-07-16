using Frontend.Models;
using Microsoft.WindowsAzure.ServiceRuntime;
using Nest;
using StretchyCloudData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Frontend.Controllers
{
	public class QueryWikipediaController : Controller
	{
		// GET: QueryWikipedia
		public ActionResult Index()
		{
			return View();
		}

		[HttpPost]
		public ActionResult Index(QueryWikipediaModel model)
		{
			if (!ModelState.IsValid)
			{
				return View();
			}
			var client = GetElasticClient();
			var results = client.Search<WebPageInfo>(s =>
				s.Size(20).Query(q => q.Match(t => t.OnField(m => m.Content).QueryString(model.QueryString))));
			model.Results.AddRange(results.Documents);
			return View(model);
		}

		private ElasticClient GetElasticClient()
		{
			var esRole = RoleEnvironment.Roles["ElasticSearch"];
			var esHosts = esRole.Instances.Select(i => i.InstanceEndpoints.First().Value.IPEndpoint.Address).ToList();
			var chosenHost = esHosts[new Random().Next(esHosts.Count)];
			var hostUri = new Uri("http://" + chosenHost + ":9200");
			var settings = new ConnectionSettings(hostUri)
				.SetDefaultIndex("wikipedia");
			return new ElasticClient(settings);
		}
	}
}