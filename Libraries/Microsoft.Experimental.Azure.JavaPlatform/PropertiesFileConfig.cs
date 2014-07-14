using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.JavaPlatform
{
	/// <summary>
	/// Constructs a standard Java properties file.
	/// </summary>
	public sealed class PropertiesFile
	{
		private readonly ImmutableList<KeyValuePair<string, string>> _configEntries;

		/// <summary>
		/// Create a new properties file.
		/// </summary>
		/// <param name="configEntires">The entries in the file.</param>
		public PropertiesFile(IEnumerable<KeyValuePair<string, string>> configEntires)
		{
			_configEntries = configEntires.ToImmutableList();
		}

		/// <summary>
		/// The entries.
		/// </summary>
		public ImmutableList<KeyValuePair<string, string>> ConfigEntries
		{
			get { return _configEntries; }
		}

		/// <summary>
		/// Returns the full text of the file defined here as a string.
		/// </summary>
		/// <returns>The full text of the file.</returns>
		public string ConstructFileText()
		{
			return String.Join("\n", _configEntries.Select(kv => String.Join("=", kv.Key, kv.Value)));
		}

		/// <summary>
		/// Writes this file out to the given location.
		/// </summary>
		/// <param name="configFilePath">The path of the file to write.</param>
		public void WriteToFile(string configFilePath)
		{
			File.WriteAllText(configFilePath,
				ConstructFileText(),
				Encoding.ASCII);
		}
	}
}
