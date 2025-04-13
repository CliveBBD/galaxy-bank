using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Api.DTOs;
using Api.Models;
using Api.Services;
using Api.Shared;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Api.Controllers
{
    [ApiController]
    [Route("disputes")]
    public class DisputesController(IDisputeService disputeService) : ControllerBase
    {
        
        private readonly IDisputeService _disputeService = disputeService;

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

                    var disputes = await _disputeService.GetAllDisputesAsync(pagination, userId, status, email);
                    return Ok(disputes);
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
                    var dispute = await _disputeService.GetDisputeAsync(disputeId);
                    if (dispute != null)
                    {
                        return Ok(dispute);
                    }
                    else
                    {
                        return NotFound(new ErrorResponse($"Dispute with disputeId={disputeId} not found."));
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

                    var disputes = await _disputeService.GetDisputeHistoryAsync(pagination, disputeId);
                    return Ok(disputes);
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
                    var createdDisputeHistoryEntry = await _disputeService.UpdateDisputeStatus(
                        disputeId,
                        disputeStatusUpdateRequest.NewStatusId,
                        disputeStatusUpdateRequest.UserID
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
                            This may be for numerous reasons including:
                            1. The dispute is not allowed to progress to dispute status disputeStatus={disputeStatusUpdateRequest.NewStatusId}.
                        """));
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
                var validationErrors = disputeCreateRequest.Validate();
                if (ModelState.IsValid && !validationErrors.Any()) 
                {
                    var createdDispute = await _disputeService.CreateDisputeAsync(
                        disputeCreateRequest.DisputedTransactionReferenceID,
                        disputeCreateRequest.Reason,
                        disputeCreateRequest.UserID,
                        disputeCreateRequest.Email
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
                    return BadRequest(ModelState.IsValid ? new ErrorResponse(validationErrors) : new ErrorResponse(ModelState));
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