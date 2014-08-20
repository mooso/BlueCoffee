using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SharkFrontEnd.Models
{
	public class SharkQueryResultModel
	{
		public int Id { get; set; }
		public string QueryString { get; set; }
		public List<dynamic> ActualResults { get; set; }
		public DateTime QuerySubmitTime { get; set; }
		public TimeSpan QueryExecutionTime { get; set; }
		public string QueryException { get; set; }
	}

	public static class AllQueries
	{
		private static readonly ConcurrentDictionary<int, SharkQueryResultModel> _allQueries =
			new ConcurrentDictionary<int, SharkQueryResultModel>();

		public static void AddNewResult(SharkQueryResultModel result)
		{
			_allQueries.TryAdd(result.Id, result);
		}

		public static SharkQueryResultModel TryGet(int id)
		{
			SharkQueryResultModel result;
			if (!_allQueries.TryGetValue(id, out result))
			{
				return null;
			}
			return result;
		}
	}
}