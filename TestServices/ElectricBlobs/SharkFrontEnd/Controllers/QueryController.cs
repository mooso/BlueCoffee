using SharkFrontEnd.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SharkFrontEnd.Controllers
{
	public class QueryController : Controller
	{
		private static int _currentId;

		// GET: Query
		public ActionResult Index()
		{
			return View();
		}

		[HttpPost]
		public ActionResult Index(SharkQueryModel query)
		{
			if (!ModelState.IsValid)
			{
				return View();
			}
			var newResult = new SharkQueryResultModel()
			{
				Id = Interlocked.Increment(ref _currentId),
				QuerySubmitTime = DateTime.Now,
				QueryString = query.QueryString,
			};
			AllQueries.AddNewResult(newResult);
			query.QueryResultId = newResult.Id;
			var executionTask = Task.Factory.StartNew(() =>
				{
					try
					{
						var dataSource = HiveDataSource.Instance;
						var timer = Stopwatch.StartNew();
						newResult.ActualResults = dataSource.ExecuteQuery(query.QueryString);
						newResult.QueryExecutionTime = timer.Elapsed;
					}
					catch (Exception ex)
					{
						newResult.QueryException = ex.Message;
					}
				});
			return View(query);
		}
	}
}