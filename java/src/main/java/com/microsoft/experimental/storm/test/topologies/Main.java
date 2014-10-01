package com.microsoft.experimental.storm.test.topologies;

import java.util.*;

import org.slf4j.*;

import storm.kafka.*;
import storm.kafka.bolt.*;
import storm.kafka.trident.*;
import storm.trident.*;
import backtype.storm.*;
import backtype.storm.generated.*;
import backtype.storm.topology.*;
import backtype.storm.tuple.*;
import backtype.storm.utils.Utils;

public class Main {
	private static final Logger LOG = LoggerFactory.getLogger(Main.class);
	private static final String KAFKA_TOPIC_NAME = "storm";

	public static void main(String[] args) throws Exception {
		StormTopology first = createGenerateToKafkaTopology();
		StormTopology second = createKafkaToBlobTopology(args[0]);
		Config conf = configure(args);
		if (args.length > 4 && args[4].equals("-local")) {
			runLocally("generate", first, conf);
			runLocally("consume", second, conf);
		} else {
			runRemote("generate", first, conf);
			runRemote("consume", second, conf);
		}
	}

	private static void runRemote(String name, StormTopology topology, Config conf) {
		while (true) {
			try {
				StormSubmitter.submitTopology(name, conf, topology);
				return;
			} catch (Exception ex) {
				// I'm being this crude now because this runs when Storm is still
				// setting up, so it's the fast-and-easy way I have to wait until Storm
				// is ready to accept my requests.
				LOG.error("Error submitting topology. Retrying.", ex);
			}
		}
	}

	private static void runLocally(String name, StormTopology toplogy, Config conf) {
		LocalCluster cluster = new LocalCluster();
		cluster.submitTopology(name, conf, toplogy);
		Utils.sleep(50000);
		cluster.killTopology(name);
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
		GlobalPartitionInformation partitionInformation = new GlobalPartitionInformation();
		partitionInformation.addPartition(0, Broker.fromString(kafkaBroker));
		TridentKafkaConfig kafkaConfig = new TridentKafkaConfig(
				new StaticHosts(partitionInformation), KAFKA_TOPIC_NAME);
		tridentTopology.newStream("source", new OpaqueTridentKafkaSpout(kafkaConfig))
			.each(new Fields("bytes"), new OutputToAzureBlob(), new Fields());

		return tridentTopology.build();
	}
}