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

namespace Populator
{
	public class WorkerRole : RoleEntryPoint
	{
		private Cluster _cluster;

		public override void Run()
		{
			try
			{
				ISession session;
				while (true)
				{
					try
					{
						session = _cluster.ConnectAndCreateDefaultKeyspaceIfNotExists();
						break;
					}
					catch (NoHostAvailableException ex)
					{
						Trace.TraceWarning(
							"No host available exception encountered. Assuming it's because we're still starting up and trying again. Message: " +
							ex.Message);
						Thread.Sleep(2000);
					}
				}
				session.Execute("create table if not exists sampletable (uid int primary key, time text)");
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
			var builder = Cluster.Builder()
				.AddContactPoints(cassandraHosts)
				.WithPort(9042)
				.WithDefaultKeyspace("sample_keyspace");
			Trace.TraceInformation("Configuring to connect to cassandra hosts: {" +
				String.Join(",", builder.ContactPoints) +
				"}.");
			_cluster = builder.Build();
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
