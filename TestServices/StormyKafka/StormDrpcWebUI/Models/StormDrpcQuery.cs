using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace StormDrpcWebUI.Models
{
	public class StormDrpcQuery
	{
		[Required]
		[Display(Name = "Space-separated word list")]
		public string QueryWords { get; set; }
		public int? QueryResult { get; set; }
	}
}