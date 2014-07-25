using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.Hive
{
	/// <summary>
	/// Configuration for a Hive metastore backed by a local Derby database.
	/// </summary>
	public sealed class HiveDerbyMetastoreConfig : HiveMetastoreConfig
	{
		private readonly string _derbyDataDirectory;

		/// <summary>
		/// Creates the Hive configuration.
		/// </summary>
		/// <param name="derbyDataDirectory">The local directory where the Derby DB files for the metastore will be stored.</param>
		/// <param name="port">The port.</param>
		public HiveDerbyMetastoreConfig(string derbyDataDirectory, int port = 9083)
			: base(port)
		{
			_derbyDataDirectory = derbyDataDirectory;
		}

		/// <summary>
		/// The configuration properties.
		/// </summary>
		protected override IEnumerable<KeyValuePair<string, string>> ConfigurationProperties
		{
			get
			{
				return new Dictionary<string, string>()
				{
					{ "javax.jdo.option.ConnectionURL", DerbyJdbcConnectionString() },
				};
			}
		}

		private string DerbyJdbcConnectionString()
		{
			return String.Format("jdbc:derby:{0};databaseName=metastore_db;create=true",
				_derbyDataDirectory.Replace('\\', '/'));
		}
	}
}
