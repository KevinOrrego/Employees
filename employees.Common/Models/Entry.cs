using System;

namespace employees.Common.Models
{
    public class Entry
    {
        public int EmployeeId { get; set; }

        public DateTime Date { get; set; }

        public DateTime Hour { get; set; }

        public int Type { get; set; }

        public bool IsConsolidated { get; set; }
    }
}
