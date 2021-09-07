
using employees.Common.Models;
using employees.Functions.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace employees.Test.Helpers
{
    public class TestFactory
    {
        public static EntryEntity GetEntryEntity()
        {
            return new EntryEntity
            {
                ETag = "*",
                PartitionKey = "ENTRY",
                RowKey = Guid.NewGuid().ToString(),
                EmployeeId = 1,
                DateHour = DateTime.UtcNow,
                IsConsolidated = false,
                Type = 1
            };
        }

        public static List<EntryEntity> GetListEntryEntity()
        {
            List<EntryEntity> list = new List<EntryEntity>();
            EntryEntity workingHoursEntity = new EntryEntity
            {
                ETag = "*",
                PartitionKey = "WORKINGHOURS",
                RowKey = Guid.NewGuid().ToString(),
                EmployeeId = 1,
                DateHour = DateTime.UtcNow,
                IsConsolidated = false,
                Type = 1
            };
            list.Add(workingHoursEntity);
            return list;
        }

        public static DefaultHttpRequest CreateHttpRequest(Guid entryId, Entry entryRequest)
        {
            string request = JsonConvert.SerializeObject(entryRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromString(request),
                Path = $"/{entryId}"
            };
        }
        public static DefaultHttpRequest CreateHttpRequest(Guid entryId)
        {
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Path = $"/{entryId}"
            };
        }

        public static DefaultHttpRequest CreateHttpRequest(Entry entryRequest)
        {
            string request = JsonConvert.SerializeObject(entryRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromString(request),
            };

        }

        public static DefaultHttpRequest CreateHttpRequest()
        {
            return new DefaultHttpRequest(new DefaultHttpContext());
        }

        public static Entry GetEntryRequest()
        {
            return new Entry
            {
                EmployeeId = 1,
                DateHour = DateTime.UtcNow,
                IsConsolidated = false,
                Type = 1
            };
        }

        public static Stream GenerateStreamFromString(string stringToConvert)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(stringToConvert);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static ILogger CreateLogger(LoggerTypes type = LoggerTypes.Null)
        {
            ILogger logger;
            if (type == LoggerTypes.List)
            {
                logger = new ListLogger();
            }
            else
            {
                logger = NullLoggerFactory.Instance.CreateLogger("Null Logger");
            }
            return logger;
        }
    }
}
