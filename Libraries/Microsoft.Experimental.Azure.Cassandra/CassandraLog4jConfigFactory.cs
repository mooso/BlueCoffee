using Microsoft.Experimental.Azure.JavaPlatform.Log4j;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.Cassandra
{
	public static class CassandraLog4jConfigFactory
	{
		public static Log4jConfig CreateConfig(string logDirectory)
		{
			var consoleAppender = AppenderDefinitionFactory.ConsoleAppender(
				layout: LayoutDefinition.PatternLayout("%5p %d{HH:mm:ss,SSS} %m%n"));
			var fileAppender = AppenderDefinitionFactory.RollingFileAppender("R",
				logDirectory.Replace('\\', '/') + "/system.log",
				maxFileSizeMb: 20,
				maxBackupIndex: 50,
				layout: LayoutDefinition.PatternLayout("%5p [%t] %d{ISO8601} %F (line %L) %m%n"));
			var rootLogger = new RootLoggerDefinition(Log4jTraceLevel.INFO, consoleAppender, fileAppender);
			var childLoggers = new[]
				{
					new ChildLoggerDefinition("org.apache.thrift.server.TNonblockingServer", Log4jTraceLevel.ERROR),
				};
			return new Log4jConfig(rootLogger, childLoggers);
		}
	}
}
