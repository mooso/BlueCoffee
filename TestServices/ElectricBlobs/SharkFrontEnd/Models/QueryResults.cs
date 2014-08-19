using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace SharkFrontEnd.Models
{
	public class QueryResults
	{
		public QueryResults()
		{
			Results = new List<dynamic>();
		}

		[Display(Name = "Query")]
		[Required]
		public string QueryString { get; set; }
		public List<dynamic> Results { get; set; }
		public string ResultsMessage { get; set; }
	}
}