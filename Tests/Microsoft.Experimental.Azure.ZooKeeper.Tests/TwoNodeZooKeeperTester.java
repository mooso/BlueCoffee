import java.io.*;
import java.util.*;

import org.apache.zookeeper.*;
import org.apache.zookeeper.ZooDefs.Ids;

public class TwoNodeZooKeeperTester {
	public static void main(String[] args) throws Exception {
		ZooKeeper zk1 = new ZooKeeper(args[0], 3000, null);
		ZooKeeper zk2 = new ZooKeeper(args[1], 3000, null);
		String path = "/simpletest";
		if (zk2.exists(path, false) != null) {
			System.err.println("Node already exists!");
			return;
		}
		zk1.create(path, new byte[0], Ids.OPEN_ACL_UNSAFE, CreateMode.EPHEMERAL);
		if (zk2.exists(path, false) == null) {
			System.err.println("Node doesn't exist after creation!");
			return;
		}
		zk1.close();
		zk2.close();
		System.out.println("Success");
	}
}