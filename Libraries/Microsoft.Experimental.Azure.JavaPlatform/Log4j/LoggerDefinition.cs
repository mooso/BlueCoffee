using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.JavaPlatform.Log4j
{
	/// <summary>
	/// Definition of a log4j logger.
	/// </summary>
	public abstract class LoggerDefinition
	{
		private readonly Log4jTraceLevel _level;
		private readonly ImmutableList<AppenderDefinition> _appenders;

		/// <summary>
		/// Creates a new definition.
		/// </summary>
		/// <param name="level">The trae level to use for this logger.</param>
		/// <param name="appenders">The appenders to use.</param>
		protected LoggerDefinition(Log4jTraceLevel level, IEnumerable<AppenderDefinition> appenders)
		{
			_level = level;
			_appenders = appenders.ToImmutableList();
		}

		/// <summary>
		/// The trace level.
		/// </summary>
		public Log4jTraceLevel Level { get { return _level; } }
		/// <summary>
		/// The appenders.
		/// </summary>
		public ImmutableList<AppenderDefinition> Appenders { get { return _appenders; } }

		/// <summary>
		/// The definition of the logger as it appears in the log4j properties.
		/// </summary>
		protected string DefinitionLine
		{
			get
			{
				return String.Join(",",
					new[] { _level.ToString() }
					.Concat(_appenders.Select(a => a.Name)));
			}
		}

		/// <summary>
		/// The log4j properties to use to get this definition.
		/// </summary>
		public abstract ImmutableDictionary<string, string> FullLog4jProperties { get; }
	}
}
