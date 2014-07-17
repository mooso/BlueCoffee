# BlueCoffee

## Description

This is an experimental set of libraries that help you run general Java programs in Microsoft Azure worker roles,
or generally from a C# program running anywhere. On top of the general library (the Azure.JavaPlatform library),
I also have specialized libraries to run some OSS projects: currently I have support for ZooKeeper, Kafka,
Cassandra and Elastic Search. Hopefully I'll add more in there (or even better people will help by expanding this with more
support).

## Quick usage

As a quick familiarity exercise with these libraries, you can quickly create a Console application that
will run a single-node Cassandra cluster. First, create an empty Console C# project. Then install
the Azure.Cassandra nuget package:

    Install-Package Microsoft.Experimental.Azure.Cassandra -IncludePrerelease

After that, put the following code as the body of your Program class. Replace the RootPath
variable with an appropriate values for your machine: RootPath is where all the Cassandra
directories will be placed.

        const string RootPath = @"C:\DeleteMe";

        private static string Q(string dir)
        {
            return Path.Combine(RootPath, dir);
        }

        static void Main(string[] args)
        {
            Directory.Delete(RootPath, recursive: true);
            var javaInstaller = new JavaInstaller(Q("Java"));
            javaInstaller.Setup();
            var config = new CassandraConfig(
                clusterName: "Tiny",
                clusterNodes: new[] { "localhost" },
                dataDirectories: new[] { Q("Data") },
                commitLogDirectory: Q("CommitLogs"),
                savedCachesDirectory: Q("SavedCaches"));
            var runner = new CassandraNodeRunner(
                jarsDirectory: Q("Jars"),
                javaHome: javaInstaller.JavaHome,
                logsDirctory: Q("Logs"),
                configDirectory: Q("Conf"),
                config: config);
            runner.Setup();
            runner.Run();
        }

Run your program. If all goes well this should just open a silent program, and if you check
the Logs directory under RootPath you'll find a system.log file showing Cassandra hopefully
happily running.