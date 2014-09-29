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

		/// <summary>
		/// Create a new definition.
		/// </summary>
		/// <param name="name">The name of the appender.</param>
		/// <param name="className">The Java fully qualified class name of the appender.</param>
		public AppenderDefinition(string name, string className)
		{
			_name = name;
			_className = className;
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
			}.Concat(CreateXmlContent()));
		}

		/// <summary>
		/// The actual XML content of the appender.
		/// </summary>
		protected abstract IEnumerable<XElement> CreateXmlContent();
	}
}
