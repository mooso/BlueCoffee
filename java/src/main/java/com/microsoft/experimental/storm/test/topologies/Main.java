package com.microsoft.experimental.storm.test.topologies;

import org.slf4j.*;

import storm.trident.*;
import storm.trident.operation.builtin.*;
import storm.trident.testing.*;
import backtype.storm.*;
import backtype.storm.generated.*;
import backtype.storm.tuple.*;
import backtype.storm.utils.Utils;

public class Main {
	private static final Logger LOG = LoggerFactory.getLogger(Main.class);

	public static void main(String[] args) throws Exception {
		StormTopology topology = createSimpleTopology();
		Config conf = configure(args);
		if (args.length > 3 && args[3].equals("-local")) {
			runLocally(topology, conf);
		} else {
			runRemote(topology, conf);
		}
	}

	private static void runRemote(StormTopology topology, Config conf) {
		while (true) {
			try {
				StormSubmitter.submitTopology("test", conf, topology);
				return;
			} catch (Exception ex) {
				// I'm being this crude now because this runs when Storm is still
				// setting up, so it's the fast-and-easy way I have to wait until Storm
				// is ready to accept my requests.
				LOG.error("Error submitting topology. Retrying.", ex);
			}
		}
	}

	private static void runLocally(StormTopology toplogy, Config conf) {
		LocalCluster cluster = new LocalCluster();
		cluster.submitTopology("test", conf, toplogy);
		Utils.sleep(50000);
		cluster.killTopology("test");
		cluster.shutdown();
	}

	private static Config configure(String[] args) {
		Config conf = new Config();
		conf.setDebug(true);
		conf.setNumWorkers(2);
		conf.put(OutputToAzureBlob.CONNECTION_STRING, args[0]);
		conf.put(OutputToAzureBlob.CONTAINER_NAME, args[1]);
		conf.put(OutputToAzureBlob.BLOB_PREFIX, args[2]);
		return conf;
	}

	@SuppressWarnings("unchecked")
	private static StormTopology createSimpleTopology() {
		FixedBatchSpout source = new FixedBatchSpout(new Fields("word"), 3,
				new Values("just"),
				new Values("testing"),
				new Values("this"),
				new Values("out"));
		source.setCycle(true);

		TridentTopology tridentTopology = new TridentTopology();
		tridentTopology.newStream("source", source)
			.groupBy(new Fields("word"))
			.aggregate(new Count(), new Fields("count"))
			.each(new Fields("word", "count"), new OutputToAzureBlob(), new Fields());

		return tridentTopology.build();
	}
}