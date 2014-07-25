using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Experimental.Azure.Hive
{
	/// <summary>
	/// Configuration for a Hive metastore backed by a SQL Server database.
	/// </summary>
	public sealed class HiveSqlServerMetastoreConfig : HiveMetastoreConfig
	{
		private readonly string _serverUri;
		private readonly string _databaseName;
		private readonly string _userName;
		private readonly string _password;

		/// <summary>
		/// Creates the configuration.
		/// </summary>
		/// <param name="serverUri">The server URI.</param>
		/// <param name="databaseName">The database name.</param>
		/// <param name="userName">The user name.</param>
		/// <param name="password">The password.</param>
		/// <param name="port">The port.</param>
		public HiveSqlServerMetastoreConfig(string serverUri, string databaseName,
			string userName, string password, int port = 9083)
			: base(port)
		{
			_serverUri = serverUri;
			_databaseName = databaseName;
			_userName = userName;
			_password = password;
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
					{ "javax.jdo.option.ConnectionURL", SqlJdbcConnctionString() },
					{ "javax.jdo.option.ConnectionDriverName", "com.microsoft.sqlserver.jdbc.SQLServerDriver" },
					{ "javax.jdo.option.ConnectionUserName", _userName},
					{ "javax.jdo.option.ConnectionPassword", _password },
				};
			}
		}

		private string SqlJdbcConnctionString()
		{
			return String.Format(
				"jdbc:sqlserver:{0};database={1};encrypt=true;trustServerCertificate=true;create=false",
				_serverUri, _databaseName);
		}
	}
}
