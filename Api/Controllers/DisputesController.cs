using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Api.DTOs;
using Api.Helpers;
using Api.Models;
using Api.Services;
using Api.Shared;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Api.Controllers
{
    [ApiController]
    [Route("disputes")]
    public class DisputesController(IDisputeService disputeService, IUserService userService) : ControllerBase
    {
        
        private readonly IDisputeService _disputeService = disputeService;
        private readonly IUserService _userService = userService;

        private static readonly ErrorResponse UnauthorizedErrorResponse = new ErrorResponse("You are not authorized to perform actions on disputes. Please log in and try again.");
        private static readonly ErrorResponse ForbiddenErrorResponse = new ErrorResponse("User is not authorized to perform this action");

        [HttpGet("", Name = "GetAllDisputes")]
        public async Task<IActionResult> GetAllDisputes(
            [FromQuery] int? userId = null,
            [FromQuery] string? email = null,
            [FromQuery] string? status = null,
            [FromQuery] int? limit = null,
            [FromQuery] int? offset = null
        )
        {
            try
            {
                if (ModelState.IsValid) 
                {
                    Pagination? pagination = new Pagination();
                    if (limit.HasValue) pagination.Limit = limit;
                    if (offset.HasValue) pagination.Offset = offset;

                    var requestingUser = await _userService.GetCurrentUser(HttpContext);

                    if (requestingUser != null && requestingUser.Role.Name == Constants.AdminRoleName)
                    {
                        var disputes = await _disputeService.GetAllDisputesAsync(pagination: pagination, userId: userId, status: status, email: email);
                        return Ok(disputes);
                    }
                    else if (requestingUser != null && requestingUser.Role.Name != Constants.AdminRoleName)
                    {
                        var disputes = await _disputeService.GetAllDisputesAsync(pagination: pagination, status: status, email: requestingUser.Email);
                        return Ok(disputes);
                    }
                    else
                    {
                        return Unauthorized(UnauthorizedErrorResponse);
                    }
                }
                else
                {
                    return BadRequest(new ErrorResponse(ModelState));
                }
            } 
            catch (Exception e)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError, 
                    new ErrorResponse(e.Message)
                );
            }
        }

        [HttpGet("{disputeId}", Name = "GetDispute")]
        public async Task<IActionResult> GetDispute(
            int disputeId
        )
        {
            try
            {
                if (ModelState.IsValid) 
                {
                    var requestingUser = await _userService.GetCurrentUser(HttpContext);

                    if (requestingUser != null && requestingUser.Role.Name == Constants.AdminRoleName)
                    {
                        var dispute = await _disputeService.GetDisputeAsync(disputeId);
                        return dispute == null ? NotFound(new ErrorResponse($"Dispute with disputeId={disputeId} not found.")) : Ok(dispute);
                    }
                    else if (requestingUser != null && requestingUser.Role.Name != Constants.AdminRoleName)
                    {
                        var dispute = await _disputeService.GetDisputeAsync(disputeId, requestingUser.UserID);
                        return dispute == null ? NotFound(new ErrorResponse($"Dispute with disputeId={disputeId} not found.")) : Ok(dispute);
                    }
                    else
                    {
                        return Unauthorized(UnauthorizedErrorResponse);
                    }
                }
                else
                {
                    return BadRequest(new ErrorResponse(ModelState));
                }
            } 
            catch (Exception e)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError, 
                    new ErrorResponse(e.Message)
                );
            }
        }

        [HttpGet("{disputeId}/history", Name = "GetDisputeStatusHistory")]
        public async Task<IActionResult> GetDisputeHistory(
            int disputeId,
            [FromQuery] int? limit = null,
            [FromQuery] int? offset = null
        )
        {
            try
            {
                if (ModelState.IsValid) 
                {
                    Pagination? pagination = new Pagination();
                    if (limit.HasValue) pagination.Limit = limit;
                    if (offset.HasValue) pagination.Offset = offset;

                    var requestingUser = await _userService.GetCurrentUser(HttpContext);

                    if (requestingUser != null && requestingUser.Role.Name == Constants.AdminRoleName)
                    {
                        var disputes = await _disputeService.GetDisputeHistoryAsync(pagination, disputeId);
                        return Ok(disputes);
                    }
                    else if (requestingUser != null && requestingUser.Role.Name != Constants.AdminRoleName)
                    {
                        var disputes = await _disputeService.GetDisputeHistoryAsync(pagination, disputeId, requestingUser.UserID);
                        return Ok(disputes);
                    }
                    else
                    {
                        return Unauthorized(UnauthorizedErrorResponse);
                    }
                }
                else
                {
                    return BadRequest(new ErrorResponse(ModelState));
                }
            } 
            catch (Exception e)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError, 
                    new ErrorResponse(e.Message)
                );
            }
        }

        [HttpGet("{disputeId}/allowed-next-statuses", Name = "GetDisputeStatuses")]
        public async Task<IActionResult> GetAllowedNextDisputeStatuses(
            int disputeId
        )
        {
            try
            {
                if (ModelState.IsValid) 
                {
                    var requestingUser = await _userService.GetCurrentUser(HttpContext);

                    if (requestingUser != null && requestingUser.Role.Name == Constants.AdminRoleName)
                    {
                        var allowedNextStatuses = await _disputeService.GetAllowedNextStatusesAsync(
                            disputeId
                        );
                        if (allowedNextStatuses != null && allowedNextStatuses.Any())
                        {
                            return Ok(allowedNextStatuses);
                        }
                        else
                        {
                            return NotFound(new ErrorResponse($"This dispute has been resolved."));
                        }
                    }
                    else
                    {
                        return StatusCode(
                            StatusCodes.Status403Forbidden, 
                            ForbiddenErrorResponse
                        );
                    }
                }
                else
                {
                    return BadRequest(new ErrorResponse(ModelState));
                }
            } 
            catch (Exception e)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError, 
                    new ErrorResponse(e.Message)
                );
            }
        }

        [HttpPost("{disputeId}/status", Name = "UpdateDisputeStatus")]
        public async Task<IActionResult> UpdateDisputeStatus(
            int disputeId,
            [FromBody] DisputeStatusUpdateRequest disputeStatusUpdateRequest
        )
        {
            try
            {
                if (ModelState.IsValid) 
                {
                    var requestingUser = await _userService.GetCurrentUser(HttpContext);

                    if (requestingUser != null && requestingUser.Role.Name == Constants.AdminRoleName)
                    {
                        var createdDisputeHistoryEntry = await _disputeService.UpdateDisputeStatus(
                            disputeId,
                            disputeStatusUpdateRequest.NewStatusId,
                            requestingUser.UserID
                        );
                        if (createdDisputeHistoryEntry != null)
                        {
                            return StatusCode(
                                StatusCodes.Status201Created, 
                                createdDisputeHistoryEntry
                            );
                        }
                        else
                        {
                            return BadRequest(new ErrorResponse($"""
                                Cannot update dispute status for disputeID={disputeId}.\n
                                This dispute is not allowed to progress to dispute status disputeStatus={disputeStatusUpdateRequest.NewStatusId}.
                            """));
                        }
                    }
                    else
                    {
                        return StatusCode(
                            StatusCodes.Status403Forbidden, 
                            ForbiddenErrorResponse
                        );
                    }
                }
                else
                {
                    return BadRequest(new ErrorResponse(ModelState));
                }
            } 
            catch (Exception e)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError, 
                    new ErrorResponse(e.Message)
                );
            }
        }

        [HttpPost("", Name = "CreateDispute")]
        public async Task<IActionResult> CreateDispute(
            [FromBody] DisputeCreateRequest disputeCreateRequest
        )
        {
            try
            {
                var requestingUser = await _userService.GetCurrentUser(HttpContext);

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ErrorResponse(ModelState));
                }
                else if (ModelState.IsValid && requestingUser != null) 
                {
                    var createdDispute = await _disputeService.CreateDisputeAsync(
                        disputeCreateRequest.DisputedTransactionReferenceID,
                        disputeCreateRequest.Reason,
                        requestingUser.UserID
                    );
                    if (createdDispute != null)
                    {
                        return StatusCode(
                            StatusCodes.Status201Created, 
                            createdDispute
                        );
                    }
                    else
                    {
                        return BadRequest(new ErrorResponse($"""
                            Cannot dispute transactionReference={disputeCreateRequest.DisputedTransactionReferenceID}.\n
                            This may be for numerous reasons including:
                            1. The transaction reference transactionReference={disputeCreateRequest.DisputedTransactionReferenceID} does not exist.
                            2. You are not involved in transactionReference={disputeCreateRequest.DisputedTransactionReferenceID}.
                            3. There is already a dispute for transactionReference={disputeCreateRequest.DisputedTransactionReferenceID}.
                            4. You may not dispute this type of transaction.
                            """));
                    }
                }
                else
                {
                    return Unauthorized(UnauthorizedErrorResponse);
                }
            } 
            catch (Exception e)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError, 
                    new ErrorResponse(e.Message)
                );
            }
        }
    }
}