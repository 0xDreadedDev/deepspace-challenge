using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;

namespace DeepSpace.Player
{
    public class GetPlayerFleet
    {
        private readonly CosmosClient _cosmosClient;

        public GetPlayerFleet(CosmosClient cosmosClient)
        {
            _cosmosClient = cosmosClient;
        }

        [FunctionName("GetPlayerFleet")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Retrieving player fleet");

            string playerAddress = req.Query["playerAddress"];
            string fleetType = req.Query["fleetType"];

            // TODO: Query container

            return new OkResult();
        }
    }
}
