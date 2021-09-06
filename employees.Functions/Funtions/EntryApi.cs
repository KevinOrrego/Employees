using employees.Common.Models;
using employees.Common.Responses;
using employees.Functions.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace employees.Functions.Funtions
{
    public static class EntryApi
    {
        [FunctionName(nameof(CreateEntry))]
        public static async Task<IActionResult> CreateEntry(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "entry")] HttpRequest req,
            [Table("entry", Connection = "AzureWebJobsStorage")] CloudTable entryTable,
            ILogger log)
        {
            log.LogInformation("new entry recieved");

            //string name = req.Query["name"];
            //dynamic data = JsonConvert.DeserializeObject(requestBody);
            //name = name ?? data?.name;

            //string responseMessage = string.IsNullOrEmpty(name)
            //    ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
            //    : $"Hello, {name}. This HTTP triggered function executed successfully.";

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Entry entry = JsonConvert.DeserializeObject<Entry>(requestBody);

            if ((entry?.EmployeeId == null) || (entry?.Type == null))
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "user id or input type is null"
                });
            }

            EntryEntity entryEntity = new EntryEntity
            {
                EmployeeId = entry.EmployeeId,
                DateHour = DateTime.UtcNow,
                Type = entry.Type,
                IsConsolidated = false,
                ETag = "*",
                PartitionKey = "ENTRY",
                RowKey = Guid.NewGuid().ToString()

            };

            TableOperation addOperation = TableOperation.Insert(entryEntity);
            await entryTable.ExecuteAsync(addOperation);

            string message = "New entry stored in table";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = "Entry added to the database",
                Result = entryEntity
            });
        }

        [FunctionName(nameof(UpdateEntry))]
        public static async Task<IActionResult> UpdateEntry(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "entry/{id}")] HttpRequest req,
            [Table("entry", Connection = "AzureWebJobsStorage")] CloudTable entryTable,
            string id,
            ILogger log)
        {
            log.LogInformation($"Update for entry:{id}, received");


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            Entry entry = JsonConvert.DeserializeObject<Entry>(requestBody);

            // Validate entry id
            TableOperation findOperation = TableOperation.Retrieve<EntryEntity>("ENTRY", id);
            TableResult findResult = await entryTable.ExecuteAsync(findOperation);

            if (findResult.Result == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = " Entry not found, try with another id"
                });
            }

            //update entry

            EntryEntity entryEntity = (EntryEntity)findResult.Result;

            //entryEntity. = entry.IsCompleted;

            if ((entry?.Type != null))
            {
                entryEntity.EmployeeId = entry.EmployeeId;
            }


            TableOperation addOperation = TableOperation.Replace(entryEntity);
            await entryTable.ExecuteAsync(addOperation);

            string message = $"Entry: {id}, has been updated";

            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = entryEntity
            });
        }

        [FunctionName(nameof(GetAllEntries))]
        public static async Task<IActionResult> GetAllEntries(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "entry")] HttpRequest req,
            [Table("entry", Connection = "AzureWebJobsStorage")] CloudTable entryTable,
            ILogger log)
        {
            log.LogInformation("Get all entries received");
            //como esto es un get, no necesito recibir el body, por lo que no necesito transformarlo

            TableQuery<EntryEntity> query = new TableQuery<EntryEntity>();
            TableQuerySegment<EntryEntity> entries = await entryTable.ExecuteQuerySegmentedAsync(query, null);

            string message = "Here are all your entries mister";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = entries
            });
        }

        [FunctionName(nameof(GetEntryById))]
        public static IActionResult GetEntryById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "entry/{id}")] HttpRequest req,
            [Table("entry", "ENTRY", "{id}", Connection = "AzureWebJobsStorage")] EntryEntity entryEntity,
            string id,
            ILogger log)
        {
            log.LogInformation($"Get entry by id: {id}, received");

            if (entryEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = " entry not found, try with another id"
                });
            }

            string message = $"Entry :{entryEntity.RowKey}, retrieved.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = entryEntity
            });
        }

        [FunctionName(nameof(DeleteEntry))]
        public static async Task<IActionResult> DeleteEntry(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "entry/{id}")] HttpRequest req,
            [Table("entry", "ENTRY", "{id}", Connection = "AzureWebJobsStorage")] EntryEntity entryEntity,
            [Table("entry", Connection = "AzureWebJobsStorage")] CloudTable entryTable,
            string id,
            ILogger log)
        {
            log.LogInformation($"Delete entry: {id}, received");

            if (entryEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = " Entry not found, try with another id"
                });
            }

            await entryTable.ExecuteAsync(TableOperation.Delete(entryEntity));

            string message = $"Entry:{entryEntity.RowKey}, deleted.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = entryEntity
            });
        }
    }
}
