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
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
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

        ////tratando de evitar que metan dos entradas seguidas

        //[FunctionName(nameof(CreateNoDuplicatedEntry))]
        //public static async Task<IActionResult> CreateNoDuplicatedEntry(
        //    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "entry")] HttpRequest req,
        //    [Table("entry", Connection = "AzureWebJobsStorage")] CloudTable entryTable,
        //    ILogger log)
        //{
        //    log.LogInformation("new entry recieved");

        //    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        //    Entry entry = JsonConvert.DeserializeObject<Entry>(requestBody);

        //    TableQuery<EntryEntity> query = new TableQuery<EntryEntity>();
        //    TableQuerySegment<EntryEntity> entries = await entryTable.ExecuteQuerySegmentedAsync(query, null);

        //    DataView dv = new DataView("ENTRY");

        //    string message = "New entry stored in table";
        //    log.LogInformation(message);

        //    return new OkObjectResult(new Response
        //    {
        //        IsSuccess = true,
        //        Message = "Entry added to the database",
        //        Result = entries
        //    });
        //}

        ////tratando de evitar que metan dos entradas seguidas

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

            EntryEntity entryEntity = (EntryEntity)findResult.Result;

            //entryEntity. = entry.IsCompleted;

            if ((entry?.Type != null) && (entry?.DateHour != null) && (entry?.EmployeeId != null))
            {
                entryEntity.EmployeeId = entry.EmployeeId;
                entryEntity.DateHour = entry.DateHour;
                entryEntity.Type = entry.Type;
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


        //Trying a consolidated consult

        [FunctionName(nameof(StartConsolidation))]
        public static async Task<IActionResult> StartConsolidation(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "consolidated-entries")] HttpRequest req,
            [Table("entry", Connection = "AzureWebJobsStorage")] CloudTable entryTable,
            ILogger log)
        {
            log.LogInformation("Get consolidated entries received");

            string notConsolidated = TableQuery.GenerateFilterConditionForBool("IsConsolidated", QueryComparisons.Equal, false);
            TableQuery<EntryEntity> query = new TableQuery<EntryEntity>().Where(notConsolidated);
            TableQuerySegment<EntryEntity> entries = await entryTable.ExecuteQuerySegmentedAsync(query, null);
            List<EntryEntity> entriesSorted = entries.OrderBy(x => x.EmployeeId).ThenBy(x => x.DateHour).ToList();

            string message = "Here are all your entries mister";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = entriesSorted
            });
        }

    }
}