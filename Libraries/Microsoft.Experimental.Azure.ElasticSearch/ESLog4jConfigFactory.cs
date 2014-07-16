using Microsoft.Experimental.Azure.JavaPlatform.Log4j;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.ElasticSearch
{
	/// <summary>
	/// Creates the default log4j configuration for Elastic Search.
	/// </summary>
	public static class ESLog4jConfigFactory
	{
		/// <summary>
		/// Creates the default configuration.
		/// </summary>
		/// <param name="logDirectory">Directory for the log files.</param>
		/// <returns>The configuration to use.</returns>
		public static Log4jConfig CreateConfig(string logDirectory)
		{
			var layout = LayoutDefinition.PatternLayout("[%d{ISO8601}][%-5p][%-25c] %m%n");

			var consoleAppender = AppenderDefinitionFactory.ConsoleAppender("console",
				layout: layout);
			var fileAppender = AppenderDefinitionFactory.DailyRollingFileAppender("file",
				Path.Combine(logDirectory, "ES.log"),
				layout: layout);
			var indexSearchSlowAppender = AppenderDefinitionFactory.DailyRollingFileAppender(
				"index_search_slow_log_file",
				Path.Combine(logDirectory, "index_search_slowlog.log"),
				layout: layout);
			var indexIndexingSlowAppender = AppenderDefinitionFactory.DailyRollingFileAppender(
				"index_indexing_slow_log_file",
				Path.Combine(logDirectory, "index_indexing_slowlog.log"),
				layout: layout);

			var rootLogger = new RootLoggerDefinition(Log4jTraceLevel.INFO, consoleAppender, fileAppender);
			var actionLogger = new ChildLoggerDefinition("action", Log4jTraceLevel.DEBUG);
			var indexSearchSlowLogger = new ChildLoggerDefinition("index.search.slowlog",
				Log4jTraceLevel.TRACE, indexIndexingSlowAppender, additivity: false);
			var indexIndexingSlowLogger = new ChildLoggerDefinition("index.indexing.slowlog",
				Log4jTraceLevel.TRACE, indexIndexingSlowAppender, additivity: false);

			return new Log4jConfig(rootLogger, new[] { actionLogger, indexSearchSlowLogger, indexIndexingSlowLogger });
		}
	}
}
