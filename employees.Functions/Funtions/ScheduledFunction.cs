using employees.Functions.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace employees.Functions.Funtions
{
    public static class ScheduledFunction
    {
        [FunctionName("ScheduledFunction")]
        public static async Task Run(
            [TimerTrigger("0 */59 * * * *")] TimerInfo myTimer,
            [Table("entry", Connection = "AzureWebJobsStorage")] CloudTable entryTable,
            [Table("consolidatedEmployee", Connection = "AzureWebJobsStorage")] CloudTable consolidatedEmployeeTable,
            ILogger log)
        {
            log.LogInformation($"Timer trigger function executed at: {DateTime.Now}");

            string notConsolidated = TableQuery.GenerateFilterConditionForBool("IsConsolidated", QueryComparisons.Equal, false);
            TableQuery<EntryEntity> query = new TableQuery<EntryEntity>().Where(notConsolidated);
            TableQuerySegment<EntryEntity> entries = await entryTable.ExecuteQuerySegmentedAsync(query, null);
            List<EntryEntity> entriesSorted = entries.OrderBy(x => x.EmployeeId).ThenBy(x => x.DateHour).ToList();

            if (entriesSorted.Count > 1)
            {

                for (int x = 0; x < entriesSorted.Count;)
                {

                    if (entriesSorted.Count == x + 1)
                    {
                        break;
                    }

                    if (entriesSorted[x].Type == 1 && entriesSorted[x + 1].Type == 0)
                    {
                        x++;
                        continue;
                    }

                    if (entriesSorted[x].EmployeeId == entriesSorted[x + 1].EmployeeId)
                    {
                        string pickById = TableQuery.GenerateFilterConditionForInt("EmployeeId", QueryComparisons.Equal, entriesSorted[x].EmployeeId);
                        TableQuery<ConsolidatedEmployeeEntity> pickEmployeeQuery = new TableQuery<ConsolidatedEmployeeEntity>().Where(pickById);
                        TableQuerySegment<ConsolidatedEmployeeEntity> currentEmployeeTime = await consolidatedEmployeeTable.ExecuteQuerySegmentedAsync(pickEmployeeQuery, null);
                        List<ConsolidatedEmployeeEntity> currentEmployeeTotal = currentEmployeeTime.Results;

                        TimeSpan timeSpan = entriesSorted[x + 1].DateHour - entriesSorted[x].DateHour;
                        DateTime currentDay = new DateTime(entriesSorted[x].DateHour.Year, entriesSorted[x].DateHour.Month, entriesSorted[x].DateHour.Day);
                        ConsolidatedEmployeeEntity consolidatedEmployeeEntity = new ConsolidatedEmployeeEntity
                        {
                            EmployeeId = entriesSorted[x].EmployeeId,
                            Date = currentDay,
                            Minutes = (int)timeSpan.TotalMinutes,
                            ETag = "*",
                            PartitionKey = "CONSOLIDATEDEMPLOYEE",
                            RowKey = Guid.NewGuid().ToString()
                        };

                        TableOperation findOperationOne = TableOperation.Retrieve<EntryEntity>("ENTRY", entriesSorted[x].RowKey);
                        TableResult findResultOne = await entryTable.ExecuteAsync(findOperationOne);
                        EntryEntity entryEntityOne = (EntryEntity)findResultOne.Result;
                        entryEntityOne.IsConsolidated = true;
                        TableOperation addOperationOne = TableOperation.Replace(entryEntityOne);
                        await entryTable.ExecuteAsync(addOperationOne);


                        TableOperation findOperationTwo = TableOperation.Retrieve<EntryEntity>("ENTRY", entriesSorted[x + 1].RowKey);
                        TableResult findResultTwo = await entryTable.ExecuteAsync(findOperationTwo);
                        EntryEntity entryEntityTwo = (EntryEntity)findResultTwo.Result;
                        entryEntityTwo.IsConsolidated = true;
                        TableOperation addOperationTwo = TableOperation.Replace(entryEntityTwo);
                        await entryTable.ExecuteAsync(addOperationTwo);



                        if (currentEmployeeTime.Results.Count == 0)
                        {
                            TableOperation addConsolidated = TableOperation.Insert(consolidatedEmployeeEntity);
                            await consolidatedEmployeeTable.ExecuteAsync(addConsolidated);
                        }
                        else
                        {
                            TableOperation findConsolidated = TableOperation.Retrieve<ConsolidatedEmployeeEntity>("CONSOLIDATEDEMPLOYEE", currentEmployeeTime.Results.ElementAt(0).RowKey);
                            TableResult consolidatedResult = await consolidatedEmployeeTable.ExecuteAsync(findConsolidated);
                            ConsolidatedEmployeeEntity consolidatedEmployee = (ConsolidatedEmployeeEntity)consolidatedResult.Result;
                            consolidatedEmployee.Minutes += (int)timeSpan.TotalMinutes;
                            TableOperation addConsolidated = TableOperation.Replace(consolidatedEmployee);
                            await consolidatedEmployeeTable.ExecuteAsync(addConsolidated);
                        }
                    }
                    x++;
                }
            }

            string message = $"Consolidation completed at: {DateTime.UtcNow}.";
            log.LogInformation(message);
        }
    }
}
