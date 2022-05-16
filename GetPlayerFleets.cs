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
using System.Collections.Generic;

namespace DeepSpace.Player
{
    public class GetPlayerFleets
    {
        private readonly CosmosClient _cosmosClient;

        public GetPlayerFleets(CosmosClient cosmosClient)
        {
            _cosmosClient = cosmosClient;
        }

        [FunctionName("GetPlayerFleets")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Searching player fleets");

            string playerAddr = req.Query["playerAddress"];
            string fleetType = req.Query["fleetType"];

            string DBName = Environment.GetEnvironmentVariable("DBName");
            Database db = (await _cosmosClient.CreateDatabaseIfNotExistsAsync(DBName)).Database;

            ContainerProperties containerProps = new ContainerProperties()
            {
                Id = Environment.GetEnvironmentVariable("FleetContainerName"),
                PartitionKeyPath = "/playerAddr",
                IndexingPolicy = new IndexingPolicy()
                {
                    Automatic = false,
                    IndexingMode = IndexingMode.Lazy
                }
            };

            Container container = (await db.CreateContainerIfNotExistsAsync(containerProps)).Container;

            using (FeedIterator setIterator = container.GetItemQueryStreamIterator(
                String.Format("SELECT f.id, f.type, f.playerAddr, f.shipIds FROM f WHERE f.type='{0}'", fleetType),
                requestOptions: new QueryRequestOptions()
                {
                    PartitionKey = new PartitionKey(playerAddr),
                    MaxConcurrency = 1,
                    MaxItemCount = 1
                }))
            {
                dynamic matchedItems = new List<dynamic>();
                while (setIterator.HasMoreResults)
                {
                    using (ResponseMessage response = await setIterator.ReadNextAsync())
                    {
                        if (!response.IsSuccessStatusCode) return new StatusCodeResult((int) response.StatusCode);
                        
                        using (StreamReader sr = new StreamReader(response.Content))
                        using (JsonTextReader jtr = new JsonTextReader(sr))
                        {
                            JsonSerializer jsonSerializer = new JsonSerializer();
                            dynamic item = jsonSerializer.Deserialize<dynamic>(jtr).Documents;
                            matchedItems.Add(item);
                        }
                    }
                }

                return new OkObjectResult(matchedItems);
            }
        }
    }
}
