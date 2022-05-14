using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;

namespace DeepSpace.Player
{
    public class PersistPlayerFleet
    {
        private readonly CosmosClient _cosmosClient;

        public PersistPlayerFleet(CosmosClient cosmosClient)
        {
            _cosmosClient = cosmosClient;
        }
        
        [FunctionName("PersistPlayerFleet")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Persisting player fleet");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic fleet = JsonConvert.DeserializeObject<PlayerFleet>(requestBody);

            string DBName = Environment.GetEnvironmentVariable("DBName");
            Database db = (await _cosmosClient.CreateDatabaseIfNotExistsAsync(DBName)).Database;

            ContainerProperties containerProps = new ContainerProperties()
            {
                Id = Environment.GetEnvironmentVariable("FleetContainerName"),
                PartitionKeyPath = "/playerAddress",
                IndexingPolicy = new IndexingPolicy()
                {
                    Automatic = false,
                    IndexingMode = IndexingMode.Lazy
                }
            };

            Container container = (await db.CreateContainerIfNotExistsAsync(containerProps)).Container;

            ItemResponse<PlayerFleet> item = await container.CreateItemAsync<PlayerFleet>(fleet, new PartitionKey(fleet.playerAddress));

            return new StatusCodeResult(((int) item.StatusCode));
        }
    }
}
