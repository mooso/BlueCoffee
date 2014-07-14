using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.JavaPlatform.Log4j
{
	/// <summary>
	/// Definition of the root logger in a log4j configuration.
	/// </summary>
	public sealed class RootLoggerDefinition : LoggerDefinition
	{
		/// <summary>
		/// Creates a new definition.
		/// </summary>
		/// <param name="level">The trace level.</param>
		/// <param name="appenders">The appenders to use.</param>
		public RootLoggerDefinition(Log4jTraceLevel level, params AppenderDefinition[] appenders)
			: base(level, appenders)
		{ }

		/// <summary>
		/// The log4j properties to use to get this definition.
		/// </summary>
		public override ImmutableDictionary<string, string> FullLog4jProperties
		{
			get
			{
				return new Dictionary<string, string>()
				{
					{ "log4j.rootLogger", DefinitionLine },
				}.ToImmutableDictionary();
			}
		}
	}
}
