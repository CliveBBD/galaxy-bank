using Api.Models;
using Api.Repositories;
using Api.Shared;
using Npgsql;

namespace Api.Services
{
    public interface IDisputeService
    {
        Task<IEnumerable<Dispute>> GetAllDisputesAsync(Pagination? pagination = null, int? userId = null, string? status = null, string? email = null);
        Task<Dispute?> GetDisputeAsync(int disputeId, int? userId = null);
        Task<IEnumerable<DisputeHistoryEntry>> GetDisputeHistoryAsync(Pagination? pagination, int disputeId, int? userId = null);
        Task<Dispute?> CreateDisputeAsync(int transactionReferenceID, string reason, int userID);
        Task<DisputeHistoryEntry?> UpdateDisputeStatus(int disputeID, int newStatusID, int updatedByID);
        Task<IEnumerable<DisputeStatus?>> GetAllowedNextStatusesAsync(int disputeID);
    }

  public class DisputeService (IDisputeRepository disputeRepository, ITransactionRepository transactionRepository) : IDisputeService
  {
    private readonly IDisputeRepository _disputeRepository = disputeRepository;
    private readonly ITransactionRepository _transactionRepository = transactionRepository;

    public async Task<IEnumerable<DisputeStatus?>> GetAllowedNextStatusesAsync(int disputeID)
    {
      return await _disputeRepository.GetAllowedNextStatusesAsync(disputeID);
    }

    public async Task<Dispute?> CreateDisputeAsync(int transactionReferenceID, string reason, int userID)
    {
      return await _disputeRepository.CreateDisputeAsync(transactionReferenceID, reason, userID);
    }

    public async Task<IEnumerable<Dispute>> GetAllDisputesAsync(Pagination? pagination, int? userId, string? status, string? email = null)
    {      
      return await _disputeRepository.GetAllDisputesAsync(pagination ?? new Pagination(), userId, status, email);
    }

    public async Task<Dispute?> GetDisputeAsync(int disputeId, int? userId = null)
    {
      return await _disputeRepository.GetDisputeAsync(disputeId, userId);
    }

    public async Task<IEnumerable<DisputeHistoryEntry>> GetDisputeHistoryAsync(Pagination? pagination, int disputeId, int? userId = null)
    {
      return await _disputeRepository.GetDisputeHistoryAsync(pagination ?? new Pagination(), disputeId, userId);
    }

    public async Task<DisputeHistoryEntry?> UpdateDisputeStatus(int disputeID, int newStatusID, int updatedByID)
    {
      bool isProgressionAllowed = await _disputeRepository.IsDisputeProgressionAllowedAsync(disputeID, newStatusID);
      Console.WriteLine("Progression allowed " + isProgressionAllowed);
      int acceptedStatusID = Constants.DisputeAcceptedId;

      if (isProgressionAllowed && newStatusID != acceptedStatusID)
      {
        return await _disputeRepository.CreateDisputeStatusHistoryEntryAsync(disputeID, newStatusID, updatedByID);
      }
      else if (isProgressionAllowed && newStatusID == acceptedStatusID)
      {        
        using var connection = new NpgsqlConnection(Constants.ConnectionString);
        await connection.OpenAsync();
        using var databaseTransaction = await connection.BeginTransactionAsync();
        try {
          var dispute = await _disputeRepository.GetDisputeAsync(disputeID);
          
          if (dispute == null) 
          {
            return null;
          }
          else
          {
            var createdDisputeHistoryEntry = await _disputeRepository.CreateDisputeStatusHistoryEntryAsync(disputeID, newStatusID, updatedByID, databaseTransaction);
            var transactionsForReference = await _transactionRepository.GetTransactionsByTransactionReferenceIdAsync(dispute.DisputedTransactionReferenceID, databaseTransaction);
            var insertedReversalTransactions = await _transactionRepository.InsertReversalTransactions(transactionsForReference.Select(transaction => transaction.TransactionID), databaseTransaction);
            
            return createdDisputeHistoryEntry;
          }

        } catch (Exception exception) {
          await databaseTransaction.RollbackAsync();
          Console.WriteLine(exception.ToString());
          throw;
        } finally {
          try
          {
            await databaseTransaction.CommitAsync();
          }
          catch
          {
            // TODO: 
          }
          await connection.CloseAsync();
        }
      }
      else
      {
        return null;
      }
    }
  }
}