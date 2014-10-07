package com.microsoft.experimental.storm.test.topologies;

import java.util.*;

import org.slf4j.*;

import storm.kafka.*;
import storm.kafka.bolt.*;
import storm.kafka.trident.*;
import storm.trident.*;
import storm.trident.operation.*;
import storm.trident.operation.builtin.*;
import storm.trident.testing.*;
import storm.trident.tuple.*;
import backtype.storm.*;
import backtype.storm.generated.*;
import backtype.storm.topology.*;
import backtype.storm.tuple.*;
import backtype.storm.utils.Utils;

public class Main {
	private static final Logger LOG = LoggerFactory.getLogger(Main.class);
	private static final String KAFKA_TOPIC_NAME = "storm";

	public static void main(String[] args) throws Exception {
		boolean runLocal = args.length > 4 && args[4].equals("-local");
		LocalDRPC drpc = null;
		if (runLocal) {
			drpc = new LocalDRPC();
		}
		List<StormTopology> topologies = new ArrayList<StormTopology>();
		topologies.add(createGenerateToKafkaTopology());
		if (Arrays.asList(args).contains("-toBlob")) {
			topologies.add(createKafkaToBlobTopology(args[0]));
		}
		topologies.add(createKafkaToDrpcTopology(args[0], drpc));
		Config conf = configure(args);
		if (runLocal) {
			runLocally(conf, topologies);
		} else {
			runRemote(conf, topologies);
		}
	}

	private static void runRemote(Config conf, List<StormTopology> topologies)
			throws InterruptedException {
		while (true) {
			try {
				for (int i = 0; i < topologies.size(); i++) {
					StormSubmitter.submitTopology("" + i, conf, topologies.get(i));
				}
				return;
			} catch (Exception ex) {
				// I'm being this crude now because this runs when Storm is still
				// setting up, so it's the fast-and-easy way I have to wait until Storm
				// is ready to accept my requests.
				LOG.error("Error submitting topology. Retrying in 1 second.", ex);
				Thread.sleep(1000);
			}
		}
	}

	private static void runLocally(Config conf, List<StormTopology> topologies) {
		LocalCluster cluster = new LocalCluster();
		for (int i = 0; i < topologies.size(); i++) {
			cluster.submitTopology("" + i, conf, topologies.get(i));
		}
		Utils.sleep(50000);
		for (int i = 0; i < topologies.size(); i++) {
			cluster.killTopology("" + i);
		}
		cluster.shutdown();
	}

	private static Config configure(String[] args) {
		Config conf = new Config();
		conf.setDebug(true);
		conf.setNumWorkers(2);
		Properties props = new Properties();
		props.put("metadata.broker.list", args[0]);
		props.put("request.required.acks", "1");
		props.put("serializer.class", "kafka.serializer.StringEncoder");
		conf.put(KafkaBolt.KAFKA_BROKER_PROPERTIES, props);
		conf.put(KafkaBolt.TOPIC, KAFKA_TOPIC_NAME);
		conf.put(OutputToAzureBlob.CONNECTION_STRING, args[1]);
		conf.put(OutputToAzureBlob.CONTAINER_NAME, args[2]);
		conf.put(OutputToAzureBlob.BLOB_PREFIX, args[3]);
		return conf;
	}

	@SuppressWarnings({ "rawtypes" })
	private static StormTopology createGenerateToKafkaTopology() {
		TopologyBuilder builder = new TopologyBuilder();
		builder.setSpout("source", new TestWordSpout(), 1);
		builder.setBolt("kafka", new KafkaBolt(), 2)
			.shuffleGrouping("source");
		return builder.createTopology();
	}

	private static StormTopology createKafkaToBlobTopology(String kafkaBroker) {
		TridentTopology tridentTopology = new TridentTopology();
		Stream kafkaStream = createKafkaTridentStream(kafkaBroker,
				tridentTopology);
		kafkaStream.each(new Fields("bytes"), new OutputToAzureBlob(), new Fields());

		return tridentTopology.build();
	}

	private static Stream createKafkaTridentStream(String kafkaBroker,
			TridentTopology tridentTopology) {
		GlobalPartitionInformation partitionInformation = new GlobalPartitionInformation();
		partitionInformation.addPartition(0, Broker.fromString(kafkaBroker));
		TridentKafkaConfig kafkaConfig = new TridentKafkaConfig(
				new StaticHosts(partitionInformation), KAFKA_TOPIC_NAME);
		return tridentTopology.newStream("source", new OpaqueTridentKafkaSpout(kafkaConfig));
	}

	private static StormTopology createKafkaToDrpcTopology(String kafkaBroker, LocalDRPC drpc) {
		TridentTopology tridentTopology = new TridentTopology();
		Stream kafkaStream = createKafkaTridentStream(kafkaBroker,
				tridentTopology);
		TridentState wordCounts = kafkaStream
			.each(new Fields("bytes"), new BaseFunction() {
				private static final long serialVersionUID = 1L;

				@Override
				public void execute(TridentTuple tuple, TridentCollector collector) {
					collector.emit(new Values(new String(tuple.getBinary(0))));
				}
			}, new Fields("word"))
		       .groupBy(new Fields("word"))
		       .persistentAggregate(new MemoryMapState.Factory(), new Count(), new Fields("count"))
		       .parallelismHint(6);
		tridentTopology.newDRPCStream("words", drpc)
	       .each(new Fields("args"), new Split(), new Fields("word"))
	       .groupBy(new Fields("word"))
	       .stateQuery(wordCounts, new Fields("word"), new MapGet(), new Fields("count"))
	       .each(new Fields("count"), new FilterNull())
	       .aggregate(new Fields("count"), new Sum(), new Fields("sum"));

		return tridentTopology.build();
	}
}