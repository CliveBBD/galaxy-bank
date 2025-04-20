// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Api.Repositories;
// using Api.Services;
// using Api.Shared;
// using NUnit.Framework;

// namespace Tests.Services
// {
//     namespace Tests.Services.DisputeServiceTest
//     {
//         public class DisputeService_GetDisputesAsync
//         {
//             private IDisputeService _disputeService;

//             [SetUp]
//             public void Setup()
//             {
//                 _disputeService = new DisputeService(new DisputeRepository());
//             }

//             [Test]
//             public async Task GetDisputesAsync_NullInputs_ReturnDisputes()
//             {
//                 // Arrange
//                 Pagination? pagination = null;
//                 int? userID = null;
//                 string? status = null;

//                 // Act
//                 var result = await _disputeService.GetAllDisputesAsync(pagination, userID, status);

//                 // Assert
//                 Assert.That(result, Is.Not.Null.And.Not.Empty, "List of disputes should not be empty");
//             }

//             [Test]
//             public async Task GetDisputesAsync_Paginated_ReturnDisputes()
//             {
//                 // Arrange
//                 int paginationLimit = 1;
//                 Pagination? pagination = new Pagination()
//                 {
//                     Limit = paginationLimit
//                 };
//                 int? userID = null;
//                 string? status = null;

//                 // Act
//                 var result = await _disputeService.GetAllDisputesAsync(pagination, userID, status);

//                 // Assert
//                 Assert.That(result, Is.Not.Null.And.Not.Empty, "List of disputes should not be empty");
//                 Assert.That(result.Count(), Is.EqualTo(paginationLimit), $"The list of disputes should have length={paginationLimit}");
//             }

//             [Test]
//             public async Task GetDisputesAsync_InputUserId_ReturnDisputes()
//             {
//                 // Arrange
//                 Pagination? pagination = null;
//                 int? userID = 1;
//                 string? status = null;

//                 // Act
//                 var result = await _disputeService.GetAllDisputesAsync(pagination, userID, status);
//                 var fullResults = await _disputeService.GetAllDisputesAsync(pagination, null, status);

//                 // Assert
//                 Assert.That(result.Count(), Is.LessThan(fullResults.Count()), "Expected list of disputes for a single user to be less than total disputes");
//             }

//             public async Task GetDisputesAsync_InputStatus_ReturnDisputes()
//             {
//                 // Arrange
//                 Pagination? pagination = null;
//                 int? userID = null;
//                 string? status = "open";

//                 // Act
//                 var result = await _disputeService.GetAllDisputesAsync(pagination, userID, status);
//                 var fullResults = await _disputeService.GetAllDisputesAsync(pagination, userID, null);

//                 // Assert
//                 Assert.That(result.Count(), Is.LessThan(fullResults.Count()), "Expected list of disputes for a single user to be less than total disputes");
//                 Assert.Multiple(() =>
//                 {
//                     foreach (var dispute in result)
//                     {
//                         Assert.That(dispute.CurrentStatus.Name, Is.EqualTo(status),
//                             $"Dispute has status {dispute.CurrentStatus.Name}, expected {status}");
//                     }
//                 });
//             }
//         }
//     }
// }