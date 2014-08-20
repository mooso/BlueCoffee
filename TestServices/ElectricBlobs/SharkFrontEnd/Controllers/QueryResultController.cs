using SharkFrontEnd.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SharkFrontEnd.Controllers
{
	public class QueryResultController : Controller
	{
		// GET: QueryResult
		public ActionResult Index(int queryId)
		{
			var result = AllQueries.TryGet(queryId);
			if (result == null)
			{
				return HttpNotFound();
			}
			return View(result);
		}
	}
}