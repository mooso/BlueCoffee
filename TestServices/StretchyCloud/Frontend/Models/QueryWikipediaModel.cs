using StretchyCloudData;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Frontend.Models
{
	public class QueryWikipediaModel
	{
		public QueryWikipediaModel()
		{
			Results = new List<WebPageInfo>();
		}

		[Display(Name = "Query")]
		[Required]
		public string QueryString { get; set; }
		public List<WebPageInfo> Results { get; private set; }
	}
}