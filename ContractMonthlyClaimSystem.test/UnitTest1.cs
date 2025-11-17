using Xunit;
//using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using ContractMonthlyClaimSystem.Controllers;
using ContractMonthlyClaimSystem.Data;
using ContractMonthlyClaimSystem.Models;
namespace ContractMonthlyClaimSystem.test
{
    public class UnitTest1
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private IFormFile CreateMockFile(string fileName, string contentType, int sizeBytes)
        {
            var content = new byte[sizeBytes];
            new Random().NextBytes(content);
            var stream = new MemoryStream(content);

            return new FormFile(stream, 0, sizeBytes, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }

        [Fact]
        public void AddClaim_ValidClaim_NoFiles_RedirectsToDashboard()
        {
            // Arrange
            var context = GetDbContext();
            var controller = new LecturerController(context);
            var claim = new Claim
            {
                FullName = "Test Lecturer",
                ModuleCode = "MOD101",
                HoursWorked = 10,
                //HourlyRate = 300,
                Total = 3000,
                Notes = "Testing claim"
            };

            // Act
            var result = controller.AddClaim(claim, null);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Dashboard", redirectResult.ActionName);
            Assert.Single(context.Claims); // claim should be added to DB
        }

        [Fact]
        public void AddClaim_WithValidFiles_SavesFilesAndDocuments()
        {
            // Arrange
            var context = GetDbContext();
            var controller = new LecturerController(context);

            var claim = new Claim
            {
                FullName = "Test Lecturer",
                ModuleCode = "MOD202",
                HoursWorked = 8,
                //HourlyRate = 300,
                Total = 2400,
                Notes = "Test with files"
            };

            var validFiles = new List<IFormFile>
            {
                CreateMockFile("test.pdf", "application/pdf", 1024),
                CreateMockFile("data.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 1024)
            };

            // Act
            var result = controller.AddClaim(claim, validFiles);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Dashboard", redirect.ActionName);
            Assert.Single(context.Claims);
            Assert.Equal(2, context.Documents.CountAsync().Result);
        }

        [Fact]
        public void AddClaim_WithInvalidFileType_ReturnsViewWithUploadError()
        {
            // Arrange
            var context = GetDbContext();
            var controller = new LecturerController(context);

            var claim = new Claim
            {
                FullName = "Invalid File Test",
                ModuleCode = "MOD303",
                HoursWorked = 5,
                //HourlyRate = 300,
                Total = 1500,
                Notes = "Invalid file type"
            };

            var invalidFiles = new List<IFormFile>
            {
                CreateMockFile("badfile.txt", "text/plain", 1024)
            };

            // Act
            var result = controller.AddClaim(claim, invalidFiles);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            Assert.NotNull(controller.ViewBag.UploadError);
            Assert.Contains("Invalid type", controller.ViewBag.UploadError.ToString());
        }

        [Fact]
        public void AddClaim_WithOversizedFile_ReturnsViewWithUploadError()
        {
            // Arrange
            var context = GetDbContext();
            var controller = new LecturerController(context);

            var claim = new Claim
            {
                FullName = "Oversized File Test",
                ModuleCode = "MOD404",
                HoursWorked = 5,
                //HourlyRate = 300,
                Total = 1500,
                Notes = "Oversized file test"
            };

            // 25 MB file (exceeds 20MB limit)
            var largeFile = new List<IFormFile>
            {
                CreateMockFile("bigfile.pdf", "application/pdf", 25 * 1024 * 1024)
            };

            // Act
            var result = controller.AddClaim(claim, largeFile);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            Assert.NotNull(controller.ViewBag.UploadError);
            Assert.Contains("Exceeds 20MB", controller.ViewBag.UploadError.ToString());
        }

    }
}
