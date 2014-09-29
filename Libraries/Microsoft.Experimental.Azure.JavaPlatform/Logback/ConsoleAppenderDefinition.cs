using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Microsoft.Experimental.Azure.JavaPlatform.Logback
{
	/// <summary>
	/// A log appender that outputs to Console.
	/// </summary>
	public sealed class ConsoleAppenderDefinition : AppenderDefinition
	{
		/// <summary>
		/// Creates a new definition.
		/// </summary>
		/// <param name="name">The name of the appender.</param>
		/// <param name="pattern">Pattern to use for each log message.</param>
		public ConsoleAppenderDefinition(string name = "stdout", string pattern = null)
			: base(name, "ch.qos.logback.core.ConsoleAppender", pattern)
		{
		}

		/// <summary>
		/// Creates the XML content.
		/// </summary>
		/// <returns></returns>
		protected override IEnumerable<XElement> CreateXmlContent()
		{
			return Enumerable.Empty<XElement>();
		}
	}
}
