using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Microsoft.Experimental.Azure.JavaPlatform.Logback
{
	/// <summary>
	/// A full logback configuration.
	/// </summary>
	public sealed class LogbackConfig
	{
		private readonly ImmutableList<ChildLoggerDefinition> _childLoggers;
		private readonly RootLoggerDefinition _rootLogger;

		/// <summary>
		/// Create a new configuration.
		/// </summary>
		/// <param name="rootLogger">The root logger definition.</param>
		/// <param name="childLoggers">The child logger definitions.</param>
		public LogbackConfig(
			RootLoggerDefinition rootLogger,
			IEnumerable<ChildLoggerDefinition> childLoggers)
		{
			_rootLogger = rootLogger;
			_childLoggers = childLoggers.ToImmutableList();
		}

		/// <summary>
		/// Create the XML document for this config.
		/// </summary>
		public XDocument ToXDocument()
		{
			var uniqueAppenders = _childLoggers.SelectMany(c => c.Appenders)
				.Concat(_rootLogger.Appenders)
				.Distinct();
			return new XDocument(
				new XElement("configuration",
					uniqueAppenders.Select(a => a.ToXmlElement())
					.Concat(new [] { _rootLogger.ToXml() })
					.Concat(_childLoggers.Select(l => l.ToXml()))
				)
			);
		}
	}
}
