package com.microsoft.experimental.storm.test.topologies;

import storm.trident.*;
import storm.trident.operation.builtin.*;
import storm.trident.testing.*;
import backtype.storm.*;
import backtype.storm.generated.*;
import backtype.storm.tuple.*;
import backtype.storm.utils.Utils;

public class Main {
	public static void main(String[] args) throws Exception {
		StormTopology topology = createSimpleTopology();
		Config conf = configure();
		while (true) {
			try {
				StormSubmitter.submitTopology("test", conf, topology);
				return;
			} catch (Exception ex) {
				// Try again (TODO: log)
				// I'm being this crude now because this runs when Storm is still
				// setting up, so it's the fast-and-easy way I have to wait until Storm
				// is ready to accept my requests.
			}
		}
	}

	@SuppressWarnings("unused")
	private static void runLocally(StormTopology toplogy, Config conf) {
		LocalCluster cluster = new LocalCluster();
		cluster.submitTopology("test", conf, toplogy);
		Utils.sleep(50000);
		cluster.killTopology("test");
		cluster.shutdown();
	}

	private static Config configure() {
		Config conf = new Config();
		conf.setDebug(true);
		conf.setNumWorkers(2);
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
			.aggregate(new Count(), new Fields("count"));
		return tridentTopology.build();
	}
}