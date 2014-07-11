using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.JavaPlatform.Log4j
{
	public static class AppenderDefinitionFactory
	{
		public static AppenderDefinition DailyRollingFileAppender(string name, string filePath,
			string datePattern = "'.'yyyy-MM-dd-HH",
			LayoutDefinition layout = null)
		{
			return new AppenderDefinition(name, "org.apache.log4j.DailyRollingFileAppender",
				new Dictionary<string, string>()
				{
					{ "DatePattern", datePattern },
					{ "File", filePath },
				}.Concat(LayoutProperties(layout)));
		}

		public static AppenderDefinition RollingFileAppender(string name, string filePath,
			int maxFileSizeMb,
			int maxBackupIndex,
			LayoutDefinition layout = null)
		{
			return new AppenderDefinition(name, "org.apache.log4j.RollingFileAppender",
				new Dictionary<string, string>()
				{
					{ "maxFileSize", maxFileSizeMb + "MB" },
					{ "maxBackupIndex", maxBackupIndex.ToString() },
					{ "File", filePath },
				}.Concat(LayoutProperties(layout)));
		}

		public static AppenderDefinition ConsoleAppender(string name = "stdout",
			LayoutDefinition layout = null)
		{
			return new AppenderDefinition(name, "org.apache.log4j.ConsoleAppender",
				LayoutProperties(layout));
		}

		private static IEnumerable<KeyValuePair<string, string>> LayoutProperties(LayoutDefinition layout)
		{
			layout = layout ?? LayoutDefinition.PatternLayout();
			return new Dictionary<string, string>()
			{
				{ "layout", layout.ClassName }
			}.Concat(layout.Properties.Select(kv =>
				new KeyValuePair<string, string>("layout." + kv.Key, kv.Value)));
		}
	}
}