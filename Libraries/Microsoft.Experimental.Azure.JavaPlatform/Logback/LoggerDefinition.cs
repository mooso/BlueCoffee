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
	/// Definition of a logback logger.
	/// </summary>
	public abstract class LoggerDefinition
	{
		private readonly LogbackTraceLevel _level;
		private readonly ImmutableList<AppenderDefinition> _appenders;

		/// <summary>
		/// Creates a new definition.
		/// </summary>
		/// <param name="level">The trae level to use for this logger.</param>
		/// <param name="appenders">The appenders to use.</param>
		protected LoggerDefinition(LogbackTraceLevel level, IEnumerable<AppenderDefinition> appenders)
		{
			_level = level;
			_appenders = appenders.ToImmutableList();
		}

		/// <summary>
		/// The trace level.
		/// </summary>
		public LogbackTraceLevel Level { get { return _level; } }
		/// <summary>
		/// The appenders.
		/// </summary>
		public ImmutableList<AppenderDefinition> Appenders { get { return _appenders; } }

		/// <summary>
		/// The XML definition.
		/// </summary>
		public abstract XElement ToXml();

		/// <summary>
		/// Creates appender ref elements
		/// </summary>
		protected IEnumerable<XElement> CreateAppenderRefElements()
		{
			return _appenders.Select(a =>
				new XElement("appender-ref",
					new XAttribute("ref", a.Name))
			);
		}
	}
}
