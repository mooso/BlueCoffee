using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Cassandra;

namespace CassandraTestApp
{
	public class WorkerRole : RoleEntryPoint
	{
		private Cluster _cluster;

		public override void Run()
		{
			try
			{
				var session = _cluster.ConnectAndCreateDefaultKeyspaceIfNotExists();
				session.Execute("create table sampletable (uid int primary key, time text)");
				long numInserted = 0;
				var timer = Stopwatch.StartNew();
				while (true)
				{
					session.Execute("insert into sampletable (uid, time) values (" + numInserted + ",'" + DateTime.Now.ToString("o") + "')");
					numInserted++;
					if (numInserted % 10000 == 0)
					{
						Trace.WriteLine("Inserted " + numInserted + " rows. Have been running for: " + timer.Elapsed);
						var lastRow = session.Execute("select * from sampletable where uid = " + (numInserted - 1)).Single();
						if (lastRow["uid"].ToString() != (numInserted - 1).ToString())
						{
							throw new InvalidOperationException("Unexpected last row UID: " + lastRow["uid"]);
						}
					}
				}
			}
			catch (Exception ex)
			{
				UploadExceptionToBlob(ex);
				throw;
			}
		}

		public override bool OnStart()
		{
			var cassandraRole = RoleEnvironment.Roles["CassandraNode"];
			var cassandraHosts = cassandraRole.Instances.Select(i => i.InstanceEndpoints.First().Value.IPEndpoint.Address).ToArray();
			_cluster = Cluster.Builder().AddContactPoints(cassandraHosts).Build();
			return base.OnStart();
		}

		private void UploadExceptionToBlob(Exception ex)
		{
			var storageAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString"));
			var container = storageAccount
					.CreateCloudBlobClient()
					.GetContainerReference("logs");
			container.CreateIfNotExists();
			container
					.GetBlockBlobReference("Exception from " + RoleEnvironment.CurrentRoleInstance.Id + " on " + DateTime.Now)
					.UploadText(ex.ToString());
		}
	}
}
