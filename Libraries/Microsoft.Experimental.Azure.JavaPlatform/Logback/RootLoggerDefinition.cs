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
	/// Definition of the root logger in a logback configuration.
	/// </summary>
	public sealed class RootLoggerDefinition : LoggerDefinition
	{
		/// <summary>
		/// Creates a new definition.
		/// </summary>
		/// <param name="level">The trace level.</param>
		/// <param name="appenders">The appenders to use.</param>
		public RootLoggerDefinition(LogbackTraceLevel level, params AppenderDefinition[] appenders)
			: base(level, appenders)
		{ }

		/// <summary>
		/// The XML definition
		/// </summary>
		public override XElement ToXml()
		{
			return new XElement("root", new XObject[]
			{
				new XAttribute("level", Level.ToString()),
			}.Concat(CreateAppenderRefElements()));
		}
	}
}
