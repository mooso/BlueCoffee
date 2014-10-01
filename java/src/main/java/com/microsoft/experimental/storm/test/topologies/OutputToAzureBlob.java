package com.microsoft.experimental.storm.test.topologies;

import java.io.IOException;
import java.net.URISyntaxException;
import java.security.InvalidKeyException;
import java.util.*;
import java.util.concurrent.atomic.*;

import org.slf4j.*;

import com.microsoft.windowsazure.storage.*;
import com.microsoft.windowsazure.storage.blob.*;

import storm.trident.operation.*;
import storm.trident.tuple.*;

public class OutputToAzureBlob implements Function {
	private static final long serialVersionUID = 1L;
	private static final Logger LOG = LoggerFactory.getLogger(OutputToAzureBlob.class);

	private CloudBlobContainer _container;
	private String _blobPrefix;
	private AtomicInteger _currentBlobIndex = new AtomicInteger();

	public static final String CONTAINER_NAME = "azure.blob.function.container";
	public static final String CONNECTION_STRING = "azure.blob.function.connection-string";
	public static final String BLOB_PREFIX = "azure.blob.function.blob-prefix";

	@Override
	public void cleanup() {
	}

	@SuppressWarnings("rawtypes")
	@Override
	public void prepare(Map conf, TridentOperationContext context) {
		String containerName = (String) conf.get(CONTAINER_NAME);
		String connectionString = (String) conf.get(CONNECTION_STRING);
		CloudStorageAccount storageAccount;
		try {
			storageAccount = CloudStorageAccount.parse(connectionString);
		} catch (InvalidKeyException e) {
			LOG.error("Invalid key in connection string: " + connectionString, e);
			return;
		} catch (URISyntaxException e) {
			LOG.error("Invalid URI in connection string: " + connectionString, e);
			return;
		}
		CloudBlobClient blobClient = storageAccount.createCloudBlobClient();
		try {
			_container = blobClient.getContainerReference(containerName);
		} catch (URISyntaxException e) {
			LOG.error("Invalid URI in trying to get container: " + containerName, e);
			return;
		} catch (StorageException e) {
			LOG.error("Storage exception in trying to get container: " + containerName, e);
			return;
		}
		try {
			_container.createIfNotExists();
		} catch (StorageException e) {
			LOG.error("Storage exception in trying to create container: " + containerName, e);
			return;
		}
		_blobPrefix = (String) conf.get(BLOB_PREFIX);
	}

	@Override
	public void execute(TridentTuple tuple, TridentCollector collector) {
		if (_container == null || _blobPrefix == null) {
			// I'm not properly prepared.
			LOG.error("Not properly prepared.");
			return;
		}
		int myIndex = _currentBlobIndex.incrementAndGet();
		String blobName = _blobPrefix + myIndex;
		try {
			CloudBlockBlob newBlob = _container.getBlockBlobReference(blobName);
			newBlob.uploadText(tuple.toString());
		} catch (URISyntaxException e) {
			LOG.error("Invalid URI trying to get blob: " + blobName, e);
			return;
		} catch (StorageException e) {
			LOG.error("Storage exception trying to write blob: " + blobName, e);
			return;
		} catch (IOException e) {
			LOG.error("IO exception trying to write blob: " + blobName, e);
			return;
		}
	}
}
