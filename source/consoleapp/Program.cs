using Microsoft.Azure.Cosmos;
using Container = Microsoft.Azure.Cosmos.Container;

namespace Cosmos_Patterns_DistributedCounter
{
    public class Program
    {
        static CosmosClient? client;

        static Database? db;

        static Container? distributedCounterContainer;

        static async Task Main(string[] args)
        {
            //create cosmos client
            client = new(
                accountEndpoint: Environment.GetEnvironmentVariable("COSMOS_ENDPOINT")!,
                authKeyOrResourceToken: Environment.GetEnvironmentVariable("COSMOS_KEY")!);

            db = await client.CreateDatabaseIfNotExistsAsync(
                id: "CounterDB"
            );

            //allocate the counter container ...
            distributedCounterContainer = await db.CreateContainerIfNotExistsAsync(
                id: "DistributedCounter",
                partitionKeyPath: "/partitionId",
                throughput: 400
            );

            while (true)
            {

                //get a name
                string name = "Product_1";
                Console.WriteLine($"What would you like the counter name to be? [{name}]:");
                string strName = Console.ReadLine();

                if (!string.IsNullOrEmpty(strName))
                {
                    name = strName;
                }

                //create distributed counter...
                int maxPartitions = 10;
                Console.WriteLine($"How many sub-docs would you like for your {name} counter? [10]:");
                string strMaxParitions = Console.ReadLine();

                try
                {
                    maxPartitions = int.Parse(strMaxParitions);
                }
                catch
                {
                    //default to 5
                }

                int initialCount = 2500;
                Console.WriteLine($"Great, creating counter {name} with {maxPartitions} sub-docs with initial count of {initialCount}");

                DistributedCounter sampleCounter = await DistributedCounter.Create(distributedCounterContainer, name, int.MaxValue, 0, maxPartitions, initialCount);

                //test that it is zero.
                int counterValues = await sampleCounter.GetCount();

                Console.WriteLine($"Counter [{name}] is: {counterValues}");

                Console.WriteLine("Press ENTER to start the simulation of high concurrency black friday sales.  Feel free to start the website to watch the counter decrement in realtime.");
                Console.ReadLine();

                Console.WriteLine($"Great, running simulation...");

                var tasks = new List<Task>();

                DateTime start = DateTime.Now;

                //increment the counter - NOTE: We are showing the addition of "1" for teh count to show the async nature of the calls.  It is likely that you would only call this once to update the count.
                for (int i = 0; i < initialCount; i++)
                {
                    try
                    {
                        //add to the counter async
                        tasks.Add(sampleCounter.UpdateCountAsync(-1));
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex.Message);
                    }
                }

                Console.WriteLine($"Would you like to attempt a min count exception? [Y]");

                string res = Console.ReadLine().ToLower();

                if (res == "y")
                {
                    try
                    {
                        //do one more than what we had stored...
                        tasks.Add(sampleCounter.UpdateCountAsync(-1));
                    }
                    catch (Exception ex)
                    { 
                        Console.Write(ex.Message); 
                    }
                }

                await Task.WhenAll(tasks);

                counterValues = await sampleCounter.GetCount();

                Console.WriteLine($"Count: {counterValues}");

                Console.WriteLine($"Press ENTER to restart, try running again with different partitions (try higher values) to see the change in performance");

                Console.ReadLine();
            }
        }
    }
}



