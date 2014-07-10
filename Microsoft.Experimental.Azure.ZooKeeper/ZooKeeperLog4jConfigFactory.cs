using JavaUtils;
using JavaUtils.Log4j;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.ZooKeeper
{
	public static class ZooKeeperLog4jConfigFactory
	{
		private const string LogDirectoryPropertyName = "zookeeper.logs.dir";

		public static Log4jConfig CreateConfig(string logDirectory)
		{
			var consoleAppender = AppenderDefinitionFactory.ConsoleAppender();
			var rootLogger = new RootLoggerDefinition(Log4jTraceLevel.INFO, consoleAppender);
			var childLoggers = new ChildLoggerDefinition[]
				{
				};
			return new Log4jConfig(LogDirectoryPropertyName, logDirectory, rootLogger, childLoggers);
		}

		private static AppenderDefinition QualifiedFileAppender(string name, string fileName)
		{
			return AppenderDefinitionFactory.FileAppender(name, "${" + LogDirectoryPropertyName + "}/" + fileName);
		}
	}
}
