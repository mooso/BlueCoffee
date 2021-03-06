import java.io.*;
import java.util.*;

import org.apache.zookeeper.*;
import org.apache.zookeeper.ZooDefs.Ids;

public class SimpleZooKeeperTester {
	public static void main(String[] args) throws Exception {
		ZooKeeper zk = new ZooKeeper("localhost:2181", 3000, null);
		String path = "/simpletest";
		if (zk.exists(path, false) != null) {
			System.err.println("Node already exists!");
			return;
		}
		zk.create(path, new byte[0], Ids.OPEN_ACL_UNSAFE, CreateMode.EPHEMERAL);
		if (zk.exists(path, false) == null) {
			System.err.println("Node doesn't exist after creation!");
			return;
		}
		zk.close();
		System.out.println("Success");
	}
}