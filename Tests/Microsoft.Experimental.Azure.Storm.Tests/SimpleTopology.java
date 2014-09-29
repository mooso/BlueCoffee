import java.io.*;
import java.util.*;

import backtype.storm.*;
import backtype.storm.generated.*;
import backtype.storm.spout.*;
import backtype.storm.task.*;
import backtype.storm.topology.*;
import backtype.storm.topology.base.*;
import backtype.storm.tuple.*;
import backtype.storm.utils.Utils;


public class SimpleTopology {

	public static void main(String[] args) throws Exception {
		StormTopology topology = createTestTopology(args[0]);
		Config conf = configure();
		StormSubmitter.submitTopology("test", conf, topology);
	}

	@SuppressWarnings("unused")
	private static void runLocally(StormTopology toplogy, Config conf) {
		LocalCluster cluster = new LocalCluster();
		cluster.submitTopology("test", conf, toplogy);
		Utils.sleep(10000);
		cluster.killTopology("test");
		cluster.shutdown();
	}

	private static Config configure() {
		Config conf = new Config();
		conf.setDebug(true);
		conf.setNumWorkers(2);
		return conf;
	}

	private static StormTopology createTestTopology(String outputFileName) {
		TopologyBuilder builder = new TopologyBuilder();
		builder.setSpout("words", new TestWordSpout(), 1);
		builder.setBolt("exclaim", new ExclamationBolt(), 1)
		        .shuffleGrouping("words");
		builder.setBolt("out", new FilePrinter(outputFileName), 1)
		        .shuffleGrouping("exclaim");
		StormTopology toplogy = builder.createTopology();
		return toplogy;
	}

	public static class FilePrinter extends BaseBasicBolt {
		private static final long serialVersionUID = 1L;
		private final String _fileName;
		private PrintStream _printStream;

		public FilePrinter(String fileName) {
			_fileName = fileName;
		}

		@Override
		public void execute(Tuple tuple, BasicOutputCollector collector) {
			_printStream.println(tuple);
			_printStream.flush();
		}

		@Override
		public void declareOutputFields(OutputFieldsDeclarer declarer) {
		}

		@Override
		public void cleanup() {
			super.cleanup();
			_printStream.close();
		}

		@SuppressWarnings("rawtypes")
		@Override
		public void prepare(Map stormConf, TopologyContext context) {
			super.prepare(stormConf, context);
			try {
				_printStream = new PrintStream(_fileName);
			} catch (FileNotFoundException e) {
				throw new Error(e);
			}
		}
	}

	public static class ExclamationBolt extends BaseBasicBolt {
		private static final long serialVersionUID = 1L;

		@Override
		public void execute(Tuple tuple, BasicOutputCollector collector) {
	        collector.emit(new Values(tuple.getString(0) + "!!!"));
	    }

		@Override
	    public void declareOutputFields(OutputFieldsDeclarer declarer) {
	        declarer.declare(new Fields("word"));
	    }
	}

	public static class TestWordSpout extends BaseRichSpout {
		private static final long serialVersionUID = 1L;
		private SpoutOutputCollector _collector;

		@Override
		public void nextTuple() {
			Utils.sleep(100);
			final String[] words = new String[] {"my", "test", "words", "are", "so", "original"};
			final Random rand = new Random();
			final String word = words[rand.nextInt(words.length)];
			_collector.emit(new Values(word));
		}

		@SuppressWarnings("rawtypes")
		@Override
		public void open(Map conf, TopologyContext context, SpoutOutputCollector collector) {
			_collector = collector;
		}

		@Override
		public void declareOutputFields(OutputFieldsDeclarer declarer) {
	        declarer.declare(new Fields("word"));
		}

		@Override
		public Map<String, Object> getComponentConfiguration() {
			return null;
		}
	}
}
