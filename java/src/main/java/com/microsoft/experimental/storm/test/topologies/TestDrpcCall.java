package com.microsoft.experimental.storm.test.topologies;

import backtype.storm.utils.DRPCClient;

public class TestDrpcCall {

	/**
	 * @param args
	 */
	public static void main(String[] args) throws Exception {
		DRPCClient client = new DRPCClient("localhost", 3772);
		System.out.println(client.execute("words", "original five my"));
	}

}
