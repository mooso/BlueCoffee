using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StretchyCloudData
{
	[ElasticType(IdProperty = "Address")]
	public class WebPageInfo
	{
		public string Address { get; set; }
		public string Content { get; set; }
	}
}
