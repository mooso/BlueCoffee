using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace SharkFrontEnd.Models
{
	public class SharkQueryModel
	{
		[Display(Name = "Query")]
		[Required]
		public string QueryString { get; set; }
		public int? QueryResultId { get; set; }
	}
}