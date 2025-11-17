using ContractMonthlyClaimSystem.Controllers;
using ContractMonthlyClaimSystem.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContractMonthlyClaimSystem.Models;

namespace ContractMonthlyClaimSystem.test
{
    public class ClaimTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public void Lecturer_AddClaim_ShouldSaveSuccessfully()
        {
            // Arrange
            var db = GetDbContext();
            var controller = new LecturerController(db);
            var claim = new Claim
            {
                UserID = 1,
                FullName = "John Doe",
                ModuleCode = "CS101",
                HoursWorked = 10,
                //HourlyRate = 200,
                Total = 2000,
                Notes = "Lecture 1"
            };

            // Act
            var result = controller.AddClaim(claim, new List<IFormFile>()) as RedirectToActionResult;

            // Assert
            var savedClaim = db.Claims.FirstOrDefault();
            Assert.NotNull(savedClaim);
            Assert.Equal("Submitted", savedClaim.ClaimStatus);
            Assert.Equal("Dashboard", result.ActionName);
        }
        [Fact]
        public void ProgramCoordinator_VerifyClaim_ShouldChangeStatusToVerified()
        {
            // Arrange
            var db = GetDbContext();

            // Seed coordinator user
            db.Users.Add(new User { UserID = 1, FullName = "Coordinator", RoleID = 2, UserEmail = "pc@test.com", Password = "pwd", ContactNumber = "0000" });
            db.SaveChanges();

            var claim = new Claim
            {
                UserID = 1,
                FullName = "John Doe",
                ModuleCode = "CS101",
                HoursWorked = 10,
                //HourlyRate = 200,
                Total = 2000,
                ClaimStatus = "Submitted",
                SubmissionDate = DateTime.Now
            };
            db.Claims.Add(claim);
            db.SaveChanges();

            var claimId = claim.ClaimID; // get generated ID
            var controller = new ProgramCoordinatorController(db);

            // Act
            var result = controller.ReviewClaim(claimId, "Verify", "Looks good") as RedirectToActionResult;

            // Assert
            var updatedClaim = db.Claims.Find(claimId);
            Assert.Equal("Verified", updatedClaim.ClaimStatus);
            Assert.Equal("Claims", result.ActionName);

            var review = db.Reviews.FirstOrDefault(r => r.ClaimID == claimId);
            Assert.NotNull(review);
            Assert.Equal("Verification", review.ReviewType);
            Assert.Equal("Verified", review.ReviewStatus);
        }


        [Fact]
        public async Task AcademicManager_ApproveClaim_ShouldChangeStatusToApproved()
        {
            // Arrange
            var db = GetDbContext();
            var claim = new Claim
            {
                ClaimID = 1,
                UserID = 1,
                FullName = "John Doe",
                ModuleCode = "CS101",
                HoursWorked = 10,
                //HourlyRate = 200,
                Total = 2000,
                ClaimStatus = "Verified",
                SubmissionDate = DateTime.Now
            };
            db.Claims.Add(claim);
            db.SaveChanges();

            var controller = new AcademicManagerController(db);

            // Act
            var result = await controller.ReviewClaim(1, "Approve", "Approved for payment") as RedirectToActionResult;

            // Assert
            var updatedClaim = db.Claims.Find(1);
            Assert.Equal("Approved", updatedClaim.ClaimStatus);
            Assert.Equal("Claims", result.ActionName);

            var review = db.Reviews.FirstOrDefault(r => r.ClaimID == 1);
            Assert.NotNull(review);
            Assert.Equal("FinalApproval", review.ReviewType);
            Assert.Equal("Approved", review.ReviewStatus);
        }

        [Fact]
        public async Task AcademicManager_RejectClaim_ShouldChangeStatusToRejected()
        {
            // Arrange
            var db = GetDbContext();
            var claim = new Claim
            {
                ClaimID = 1,
                UserID = 1,
                FullName = "John Doe",
                ModuleCode = "CS101",
                HoursWorked = 10,
                //HourlyRate = 200,
                Total = 2000,
                ClaimStatus = "Verified",
                SubmissionDate = DateTime.Now
            };
            db.Claims.Add(claim);
            db.SaveChanges();

            var controller = new AcademicManagerController(db);

            // Act
            var result = await controller.ReviewClaim(1, "Reject", "Incorrect hours") as RedirectToActionResult;

            // Assert
            var updatedClaim = db.Claims.Find(1);
            Assert.Equal("Rejected", updatedClaim.ClaimStatus);
            Assert.Equal("Claims", result.ActionName);

            var review = db.Reviews.FirstOrDefault(r => r.ClaimID == 1);
            Assert.NotNull(review);
            Assert.Equal("FinalApproval", review.ReviewType);
            Assert.Equal("Rejected", review.ReviewStatus);
        }
    }
}