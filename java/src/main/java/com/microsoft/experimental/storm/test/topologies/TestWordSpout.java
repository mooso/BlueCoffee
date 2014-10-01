package com.microsoft.experimental.storm.test.topologies;

import java.util.*;

import backtype.storm.spout.*;
import backtype.storm.task.*;
import backtype.storm.topology.*;
import backtype.storm.topology.base.*;
import backtype.storm.tuple.*;
import backtype.storm.utils.Utils;


public class TestWordSpout extends BaseRichSpout {
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
        declarer.declare(new Fields("message"));
	}

	@Override
	public Map<String, Object> getComponentConfiguration() {
		return null;
	}
}
