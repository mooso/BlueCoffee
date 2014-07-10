using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.JavaPlatform.Log4j
{
	public sealed class RootLoggerDefinition : LoggerDefinition
	{
		public RootLoggerDefinition(Log4jTraceLevel level, AppenderDefinition appender)
			: base(level, appender)
		{ }

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
