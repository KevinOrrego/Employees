using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace employees.Test.Helpers
{
    public class MockCloudTableConsolidatedEmployees : CloudTable
    {
        public MockCloudTableConsolidatedEmployees(Uri tableAddress) : base(tableAddress)
        {
        }

        public MockCloudTableConsolidatedEmployees(Uri tableAbsoluteUri, StorageCredentials credentials) : base(tableAbsoluteUri, credentials)
        {
        }

        public MockCloudTableConsolidatedEmployees(StorageUri tableAddress, StorageCredentials credentials) : base(tableAddress, credentials)
        {
        }

        public override async Task<TableResult> ExecuteAsync(TableOperation operation)
        {
            return await Task.FromResult(new TableResult
            {
                HttpStatusCode = 200,
                Result = TestFactory.GetEntryEntity()
            });
        }
        public override async Task<TableQuerySegment<ConsolidatedEmployeeEntity>> ExecuteQuerySegmentedAsync<ConsolidatedEmployeeEntity>(TableQuery<ConsolidatedEmployeeEntity> query, TableContinuationToken token)
        {
            ConstructorInfo constructor = typeof(TableQuerySegment<ConsolidatedEmployeeEntity>)
                   .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                   .FirstOrDefault(c => c.GetParameters().Count() == 1);

            return await Task.FromResult(constructor.Invoke(new object[] { TestFactory.GetListConsolidatedEmployeeEntity() }) as TableQuerySegment<ConsolidatedEmployeeEntity>);
        }
    }
}
