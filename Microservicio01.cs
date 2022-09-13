using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Company.Function
{
    public static class Microservicio01
    {
        [FunctionName("Microservicio01")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var dbName = System.Environment.GetEnvironmentVariable("COSMOSDB_DATABASE_NAME", EnvironmentVariableTarget.Process);
            var str = GetCustomConnectionString("CosmosDBConnectionString");
                        
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            var _guid = Guid.NewGuid();
            var mongoClient = new MongoClient(str);
            var mongoDb = mongoClient.GetDatabase(dbName);
            var collection = mongoDb.GetCollection<BsonDocument>("col_tx01");

            var document = new BsonDocument
            {
                { "name", name },
                { "type", "Microservice 1" },
                { "guid", _guid.ToString() },
                { "created_at", DateTime.UtcNow }
            };

            await collection.InsertOneAsync(document);

            string responseMessage = string.IsNullOrEmpty(name)
                ? "Importante! Debes proveernos con un nombre."
                : $"Hola, {name}. Esta transacción de tipo Microservicio 1 se ha registrado correctamente en la base de dato. ID: {_guid}";

            return new OkObjectResult(responseMessage);
        }

        public static string GetCustomConnectionString(string name)
        {
            string conStr = System.Environment.GetEnvironmentVariable($"ConnectionStrings:{name}", EnvironmentVariableTarget.Process);
            if (string.IsNullOrEmpty(conStr)) // Azure Functions App Service naming convention
                conStr = System.Environment.GetEnvironmentVariable($"CUSTOMCONNSTR_{name}", EnvironmentVariableTarget.Process);
            return conStr;
        }
    }
}
