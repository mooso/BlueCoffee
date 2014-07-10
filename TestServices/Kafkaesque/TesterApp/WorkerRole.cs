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
using System.Collections.Immutable;
using Kafka.Client;

namespace TesterApp
{
	public class WorkerRole : RoleEntryPoint
	{
		private ImmutableList<string> _brokerHosts;
		private const int KafkaPort = 9092;

		public override void Run()
		{
			Connector connector;
			int brokerHostIndex = 0;
			var clientId = RoleEnvironment.CurrentRoleInstance.Id;
			var correlationId = 0;
			var partitionId = 0;
			var topicName = "sampletopic";
			while (true)
			{
				try
				{
					connector = new Connector(_brokerHosts[brokerHostIndex], KafkaPort);
					var metadata = connector.Metadata(correlationId, clientId, "sampletopic");
					break;
				}
				catch (Exception ex)
				{
					Trace.TraceError("Can't connect to Kafka, assuming it's still not up and running. Exception: " + ex);
				}
				brokerHostIndex = (brokerHostIndex + 1) % _brokerHosts.Count;
			}
			Trace.TraceInformation("Connected to Kafka broker " + _brokerHosts[brokerHostIndex]);
			long numProduced = 0;
			while (true)
			{
				var produceResponse = connector.Produce(correlationId, clientId, 500, topicName, partitionId, new byte[64]);
				numProduced++;
				if (numProduced % 10000 == 0)
				{
					Trace.TraceInformation("Produced " + numProduced + " messages");
				}
			}
		}

		public override bool OnStart()
		{
			var brokerRole = RoleEnvironment.Roles["KafkaBroker"];
			_brokerHosts = brokerRole.Instances.Select(i => i.InstanceEndpoints.First().Value.IPEndpoint.Address.ToString()).ToImmutableList();
			return base.OnStart();
		}
	}
}
