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
	/// Definition for an appender for logback.
	/// </summary>
	public abstract class AppenderDefinition
	{
		private readonly string _name;
		private readonly string _className;
		private readonly string _pattern;

		/// <summary>
		/// Create a new definition.
		/// </summary>
		/// <param name="name">The name of the appender.</param>
		/// <param name="className">The Java fully qualified class name of the appender.</param>
		/// <param name="pattern">Pattern to use for each log message.</param>
		public AppenderDefinition(string name, string className, string pattern = null)
		{
			_name = name;
			_className = className;
			_pattern = pattern ?? "%d{yyyy-MM-dd HH:mm:ss} %c{1} [%p] %m%n";
		}

		/// <summary>
		/// Name of the appender.
		/// </summary>
		public string Name { get { return _name; } }
		/// <summary>
		/// The Java fully qualified class name of the appender.
		/// </summary>
		public string ClassName { get { return _className; } }

		/// <summary>
		/// The XML definition.
		/// </summary>
		public XElement ToXmlElement()
		{
			return new XElement("appender", new XObject[]
			{
				new XAttribute("name", _name),
				new XAttribute("class", _className),
				EncoderElement(),
			}.Concat(CreateXmlContent()));
		}

		/// <summary>
		/// The encoder XML element.
		/// </summary>
		/// <returns></returns>
		private XElement EncoderElement()
		{
			return new XElement("encoder",
				new XElement("pattern", _pattern))
			;
		}

		/// <summary>
		/// The actual XML content of the appender.
		/// </summary>
		protected abstract IEnumerable<XElement> CreateXmlContent();
	}
}
