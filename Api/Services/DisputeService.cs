using Api.Models;
using Api.Repositories;
using Api.Shared;

namespace Api.Services
{
    public interface IDisputeService
    {
        Task<IEnumerable<Dispute>> GetAllDisputesAsync(Pagination? pagination, int? userId, string? status, string? email = null);
        Task<Dispute?> GetDisputeAsync(int disputeId);
        Task<IEnumerable<DisputeHistoryEntry>> GetDisputeHistoryAsync(Pagination? pagination, int disputeId);
        Task<Dispute?> CreateDisputeAsync(int transactionReferenceID, string reason, int? userID = null, string? email = null);
        Task<DisputeHistoryEntry?> UpdateDisputeStatus(int disputeID, int newStatusID, int updatedByID);
    }

  public class DisputeService (IDisputeRepository disputeRepository) : IDisputeService
  {
    private readonly IDisputeRepository _disputeRepository = disputeRepository;

    public async Task<Dispute?> CreateDisputeAsync(int transactionReferenceID, string reason, int? userID = null, string? email = null)
    {
      // TODO: add currently logged in user logic (check that the provided user id == the currently logged in user id)
      if (userID == null && email == null) 
      {
        throw new ArgumentException("A userID or email must be provided.");
      }
      else
      {
        return await _disputeRepository.CreateDisputeAsync(transactionReferenceID, reason, userID, email);
      }
    }

    public async Task<IEnumerable<Dispute>> GetAllDisputesAsync(Pagination? pagination, int? userId, string? status, string? email = null)
    {      
      // TODO: add currently logged in user logic:
      /*
      * User may only view disputes that they are involved in.
      * Admins may view all disputes
      */
      return await _disputeRepository.GetAllDisputesAsync(pagination ?? new Pagination(), userId, status, email);
    }

    public async Task<Dispute?> GetDisputeAsync(int disputeId)
    {
      // TODO: add currently logged in user logic:
      /*
      * User may only view disputes that they are involved in.
      * Admins may view all disputes
      */
      return await _disputeRepository.GetDisputeAsync(disputeId);
    }

    public async Task<IEnumerable<DisputeHistoryEntry>> GetDisputeHistoryAsync(Pagination? pagination, int disputeId)
    {
      // TODO: add currently logged in user logic:
      /*
      * Admin or Dispute Resolution Officer may view all histories
      * User may only see history of disputes they are involved in
      */
      return await _disputeRepository.GetDisputeHistoryAsync(pagination ?? new Pagination(), disputeId);
    }

    public async Task<DisputeHistoryEntry?> UpdateDisputeStatus(int disputeID, int newStatusID, int updatedByID)
    {
      // TODO: add currently logged in user logic:
      /*
      * Admin only is allowed to update dispute status
      */
      bool isProgressionAllowed = await _disputeRepository.IsDisputeProgressionAllowedAsync(disputeID, updatedByID);
      int acceptedStatusID = int.TryParse(Environment.GetEnvironmentVariable("ACCEPTED_STATUS_ID"), out var result) ? result : 3;

      if (isProgressionAllowed && newStatusID != acceptedStatusID)
      {
        return await _disputeRepository.CreateDisputeStatusHistoryEntryAsync(disputeID, newStatusID, updatedByID);
      }
      else if (isProgressionAllowed && newStatusID == acceptedStatusID)
      {
        //TODO: If a dispute is approved, the system must create reciprocal transactions for each transaction in the transaction reference
        throw new NotImplementedException();
      }
      else
      {
        return null;
      }
    }
  }
}