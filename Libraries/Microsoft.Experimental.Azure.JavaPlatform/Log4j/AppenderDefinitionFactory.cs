using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.JavaPlatform.Log4j
{
	/// <summary>
	/// Factory class for common appenders in Log4j.
	/// </summary>
	public static class AppenderDefinitionFactory
	{
		/// <summary>
		/// Definition for a daily-rolling file appender.
		/// </summary>
		/// <param name="name">The name of the appender.</param>
		/// <param name="filePath">The path of the file.</param>
		/// <param name="datePattern">The pattern to use when naming that file.</param>
		/// <param name="layout">The layout to use when writing messages in that file.</param>
		/// <returns>The defintion.</returns>
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

		/// <summary>
		/// Definition for a rolling file appender.
		/// </summary>
		/// <param name="name">The name of the appender.</param>
		/// <param name="filePath">The path of the file.</param>
		/// <param name="maxFileSizeMb">Maximum file size in MB.</param>
		/// <param name="maxBackupIndex">Maximum index for backup files.</param>
		/// <param name="layout">The layout to use when writing messages in that file.</param>
		/// <returns>The definition.</returns>
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

		/// <summary>
		/// Definition for a console (stdout) appender.
		/// </summary>
		/// <param name="name">The name of the appender.</param>
		/// <param name="layout">The layout to use when writing messages in that file.</param>
		/// <returns>The definition.</returns>
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