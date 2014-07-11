using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.JavaPlatform.Log4j
{
	public sealed class Log4jConfig
	{
		private readonly ImmutableList<KeyValuePair<string, string>> _definedProperties;
		private readonly ImmutableList<ChildLoggerDefinition> _childLoggers;
		private readonly RootLoggerDefinition _rootLogger;

		public Log4jConfig(
			RootLoggerDefinition rootLogger,
			IEnumerable<ChildLoggerDefinition> childLoggers,
			IEnumerable<KeyValuePair<string, string>> definedProperties = null)
		{
			_definedProperties =
				(definedProperties ?? Enumerable.Empty<KeyValuePair<string, string>>())
				.ToImmutableList();
			_rootLogger = rootLogger;
			_childLoggers = childLoggers.ToImmutableList();
		}

		public PropertiesFile ToPropertiesFile()
		{
			var uniqueAppenders = _childLoggers.SelectMany(c => c.Appenders)
				.Concat(_rootLogger.Appenders)
				.Distinct();
			var appenderDefinitions = uniqueAppenders.SelectMany(a => a.FullLog4jProperties);
			return new PropertiesFile(
				_definedProperties
				.Concat(_rootLogger.FullLog4jProperties)
				.Concat(appenderDefinitions)
				.Concat(_childLoggers.SelectMany(a => a.FullLog4jProperties)));
		}
	}
}
