using JavaUtils.Log4j;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavaUtils
{
	public sealed class Log4jConfig
	{
		private readonly string _logDirectory;
		private readonly string _logDirectoryPropertyName;
		private readonly ImmutableList<ChildLoggerDefinition> _childLoggers;
		private readonly RootLoggerDefinition _rootLogger;

		public Log4jConfig(string logDirectoryPropertyName,
			string logDirectory,
			RootLoggerDefinition rootLogger,
			IEnumerable<ChildLoggerDefinition> childLoggers)
		{
			_logDirectoryPropertyName = logDirectoryPropertyName;
			_logDirectory = logDirectory;
			_rootLogger = rootLogger;
			_childLoggers = childLoggers.ToImmutableList();
		}

		public PropertiesFile ToPropertiesFile()
		{
			var uniqueAppenders = _childLoggers.Select(c => c.Appender)
				.Concat(new[] { _rootLogger.Appender })
				.Distinct();
			var appenderDefinitions = uniqueAppenders.SelectMany(a => a.FullLog4jProperties);
			var logDirectoryProperty = new Dictionary<string, string>() { { _logDirectoryPropertyName, _logDirectory.Replace('\\', '/') } };
			return new PropertiesFile(
				logDirectoryProperty
				.Concat(_rootLogger.FullLog4jProperties)
				.Concat(appenderDefinitions)
				.Concat(_childLoggers.SelectMany(a => a.FullLog4jProperties)));
		}
	}
}
