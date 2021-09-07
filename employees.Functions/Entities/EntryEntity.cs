using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace employees.Functions.Entities
{
    public class EntryEntity : TableEntity
    {
        public int EmployeeId { get; set; }

        public DateTime DateHour { get; set; }

        public int Type { get; set; }

        public bool IsConsolidated { get; set; }
    }
}
