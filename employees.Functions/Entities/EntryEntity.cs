using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace employees.Functions.Entities
{
    public class EntryEntity : TableEntity
    {
        public int EmployeeId { get; set; }

        public DateTime Date { get; set; }

        public DateTime Hour { get; set; }

        public int Type { get; set; }

        public bool IsConsolidated { get; set; }
    }
}
