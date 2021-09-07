
using employees.Common.Models;
using employees.Functions.Entities;
using employees.Functions.Funtions;
using employees.Test.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Xunit;

namespace employees.Test.tests
{
    public class EntryApiTest
    {
        private readonly ILogger logger = TestFactory.CreateLogger();

        [Fact]
        public async void CreateEntry_Should_Return_200()
        {
            // Arrange
            MockCloudTableEntries mockEntries = new MockCloudTableEntries(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            Entry entryRequest = TestFactory.GetEntryRequest();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(entryRequest);

            // Act

            IActionResult response = await EntryApi.CreateEntry(request, mockEntries, logger);

            // Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

        }

        [Fact]
        public async void UpdateEntry_Should_Return_200()
        {
            // Arrange
            MockCloudTableEntries mockEntries = new MockCloudTableEntries(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            Entry entryRequest = TestFactory.GetEntryRequest();
            Guid entryId = Guid.NewGuid();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(entryId, entryRequest);

            // Act

            IActionResult response = await EntryApi.UpdateEntry(request, mockEntries, entryId.ToString(), logger);

            // Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

        }

        [Fact]
        public async void GetEntries_Should_Return_200()
        {
            // Arrange
            MockCloudTableEntries mockEntries = new MockCloudTableEntries(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            Entry entryRequest = TestFactory.GetEntryRequest();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(entryRequest);

            // Act

            IActionResult response = await EntryApi.GetAllEntries(request, mockEntries, logger);

            // Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

        }

        [Fact]
        public async void GetEntryById_Should_Return_200()
        {
            // Arrange
            MockCloudTableEntries mockEntries = new MockCloudTableEntries(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            Entry entryRequest = TestFactory.GetEntryRequest();
            Guid entryId = Guid.NewGuid();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(entryId, entryRequest);

            // Act

            IActionResult response = await EntryApi.GetEntryById(request, mockEntries, entryId.ToString(), logger);

            // Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

        }

        [Fact]
        public async void DeleteEntryById_Should_Return_200()
        {
            // Arrange
            MockCloudTableEntries mockEntries = new MockCloudTableEntries(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            Entry entryRequest = TestFactory.GetEntryRequest();
            Guid entryId = Guid.NewGuid();
            EntryEntity entryEntity = TestFactory.GetEntryEntity();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(entryId, entryRequest);

            // Act

            IActionResult response = await EntryApi.DeleteEntry(request,entryEntity, mockEntries, entryId.ToString(), logger);

            // Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

        }
    }
}
