import java.util.*;

import backtype.storm.*;
import backtype.storm.generated.*;
import backtype.storm.spout.*;
import backtype.storm.task.*;
import backtype.storm.topology.*;
import backtype.storm.tuple.*;
import backtype.storm.utils.Utils;


public class SimpleTopology {

	public static void main(String[] args) throws Exception {
		StormTopology topology = createTestTopology();
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

	private static StormTopology createTestTopology() {
		TopologyBuilder builder = new TopologyBuilder();
		builder.setSpout("words", new TestWordSpout(), 10);
		builder.setBolt("exclaim1", new ExclamationBolt(), 3)
		        .shuffleGrouping("words");
		builder.setBolt("exclaim2", new ExclamationBolt(), 2)
		        .shuffleGrouping("exclaim1");
		StormTopology toplogy = builder.createTopology();
		return toplogy;
	}

	public static class ExclamationBolt implements IRichBolt {
		private static final long serialVersionUID = 1L;
		private OutputCollector _collector;

		@SuppressWarnings("rawtypes")
		@Override
		public void prepare(Map conf, TopologyContext context, OutputCollector collector) {
	        _collector = collector;
	    }

		@Override
	    public void execute(Tuple tuple) {
	        _collector.emit(tuple, new Values(tuple.getString(0) + "!!!"));
	        _collector.ack(tuple);
	    }

		@Override
	    public void cleanup() {
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

	public static class TestWordSpout implements IRichSpout {
		private static final long serialVersionUID = 1L;
		private SpoutOutputCollector _collector;

		@Override
		public void ack(Object msgId) {
		}

		@Override
		public void activate() {
		}

		@Override
		public void close() {
		}

		@Override
		public void deactivate() {
		}

		@Override
		public void fail(Object msgId) {
		}

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
