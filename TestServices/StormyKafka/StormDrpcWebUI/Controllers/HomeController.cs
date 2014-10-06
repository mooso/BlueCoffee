using StormDrpcWebUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StormDrpcWebUI.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			return View();
		}

		public ActionResult Query(StormDrpcQuery model)
		{
			if (ModelState.IsValid)
			{
				model.QueryResult = DrpcQuery.Instance.GetWordCount(model.QueryWords);
				return View("Index", model);
			}
			return View("Index", model);
		}
	}
}