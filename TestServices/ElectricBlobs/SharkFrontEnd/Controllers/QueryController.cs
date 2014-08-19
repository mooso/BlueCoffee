using SharkFrontEnd.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SharkFrontEnd.Controllers
{
	public class QueryController : Controller
	{
		// GET: Query
		public ActionResult Index()
		{
			return View();
		}

		[HttpPost]
		public ActionResult Index(QueryResults query)
		{
			if (!ModelState.IsValid)
			{
				return View();
			}
			if (HiveDataSource.Instance == null)
			{
				query.ResultsMessage = "Not yet initialized...";
				return View(query);
			}
			query.Results = HiveDataSource.Instance.ExecuteQuery(query.QueryString);
			return View(query);
		}
	}
}