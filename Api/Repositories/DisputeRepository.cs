using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Models;
using Api.Shared;
using Dapper;
using Npgsql;

namespace Api.Repositories
{
        public interface IDisputeRepository
    {
        public Task<IEnumerable<Dispute>> GetAllDisputesAsync(Pagination pagination, int? userId = null, string? status = null, string? email = null);
        public Task<Dispute?> GetDisputeAsync(int disputeId, int? userId = null);
        public Task<IEnumerable<DisputeHistoryEntry>> GetDisputeHistoryAsync(Pagination pagination, int disputeId, int? userId = null);
        public Task<Dispute?> CreateDisputeAsync(int transactionReferenceID, string reason, int userID);
        public Task<bool> IsDisputeProgressionAllowedAsync(int disputeID, int newStatusID);
        public Task<DisputeHistoryEntry?> CreateDisputeStatusHistoryEntryAsync(int disputeID, int newStatusID, int updatedByID, NpgsqlTransaction? transaction = null);
        Task<IEnumerable<DisputeStatus>> GetAllowedNextStatusesAsync(int disputeId);
    }

  public class DisputeRepository : IDisputeRepository
  {
    private static readonly Func<Dispute, DisputeStatus, Dispute> _disputeMapper = (dispute, disputeStatus) =>
    {
        dispute.CurrentStatus ??= new DisputeStatus(){ DisputeStatusID = disputeStatus.DisputeStatusID, Name = disputeStatus.Name };
        dispute.CurrentStatus.DisputeStatusID = disputeStatus.DisputeStatusID;
        dispute.CurrentStatus.Name = disputeStatus.Name;
        return dispute;
    };

    private static Func<DisputeHistoryEntry, RedactedUser, DisputeStatus, DisputeHistoryEntry> _disputeHistoryEntryMapper = (disputeHistoryEntry, redactedUser, disputeStatus) =>
    {
        disputeHistoryEntry.UpdatedBy ??= new RedactedUser()
        { 
            UserID = redactedUser.UserID,
            Username = redactedUser.Username,
            Email = redactedUser.Email
        };
        disputeHistoryEntry.Status ??= new DisputeStatus()
        { 
            DisputeStatusID = disputeStatus.DisputeStatusID, 
            Name = disputeStatus.Name 
        };
        return disputeHistoryEntry;
    };
    public async Task<Dispute?> CreateDisputeAsync(int transactionReferenceID, string reason, int userID)
    {
        string insertDisputeQuery = $@"
            WITH
                candidate_transactions_for_user AS (
                    SELECT t.transaction_reference_id, t.transaction_type_id, tt.name as transaction_type_name
                    FROM transactions t
                        INNER JOIN accounts a ON t.account_id = a.account_id
                        INNER JOIN transaction_types tt ON t.transaction_type_id = tt.transaction_type_id
                        INNER JOIN users u ON a.user_id = u.user_id
                    WHERE t.transaction_reference_id = @transactionReferenceId
                        AND (@userId IS NOT NULL AND a.user_id = @userId)
                ),
                undisputable_transaction_references_for_user AS (
                    SELECT transaction_reference_id
                    FROM candidate_transactions_for_user
                    WHERE transaction_type_name != 'transfer_out' --only transfer_out can be disputed
                    UNION ALL
                    SELECT disputed_transaction_reference_id
                    FROM disputes
                    WHERE disputed_transaction_reference_id = @transactionReferenceId
                ),
                disputable_transaction_for_user AS (
                    SELECT transaction_reference_id
                    FROM candidate_transactions_for_user
                    WHERE transaction_reference_id NOT IN (SELECT transaction_reference_id FROM undisputable_transaction_references_for_user)
                )
                INSERT INTO disputes (reason, disputed_transaction_reference_id, created_at)
                SELECT @reason, transaction_reference_id, NOW()
                FROM disputable_transaction_for_user
                RETURNING dispute_id;
        ";

        string insertDisputeHistoryQuery = $@"
        	INSERT INTO dispute_status_history (dispute_id, dispute_status_id, updated_at, updated_by_id)
            SELECT @disputeID, 1, NOW(), user_id
            FROM users
            WHERE (@userId IS NOT NULL AND user_id = @userId)
            RETURNING dispute_history_id;
        ";

        string getCreatedDisputeQuery = $@"
            SELECT 
                d.dispute_id AS { nameof(Dispute.DisputeID) },
                d.reason AS { nameof(Dispute.Reason) },
                d.disputed_transaction_reference_id AS { nameof(Dispute.DisputedTransactionReferenceID) },
                d.created_at AS { nameof(Dispute.CreatedAt) },
                dsh.dispute_status_id AS { nameof(Dispute.CurrentStatus.DisputeStatusID) },
                ds.name AS { nameof(Dispute.CurrentStatus.Name) }
            FROM disputes d
				INNER JOIN dispute_status_history dsh ON d.dispute_id = dsh.dispute_id
				INNER JOIN dispute_statuses ds ON dsh.dispute_status_id = ds.dispute_status_id
            WHERE dsh.dispute_history_id = @disputeHistoryID
        ";

        using var connection = new NpgsqlConnection(Constants.ConnectionString);
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            
            var insertDisputeQueryParameters = new
            {
                transactionReferenceID,
                reason,
                userID
            };

            var disputeID = await connection.ExecuteScalarAsync<int>(insertDisputeQuery, insertDisputeQueryParameters, transaction);

            if (disputeID == 0)
            {
                await transaction.RollbackAsync();
                return null;
            }
            else
            {
                var insertDisputeHistoryQueryParameters = new
                {
                    disputeID,
                    userID
                };
                var disputeHistoryID = await connection.ExecuteScalarAsync<int>(insertDisputeHistoryQuery, insertDisputeHistoryQueryParameters, transaction);
                
                if (disputeHistoryID == 0)
                {                  
                    await transaction.RollbackAsync();
                    return null;
                }
                else
                {
                    var getCreatedDisputeQueryParameters = new
                    {
                        disputeHistoryID,
                    };
                    var createdDispute = (await connection.QueryAsync<Dispute, DisputeStatus, Dispute>(
                        getCreatedDisputeQuery,
                        _disputeMapper,
                        param: getCreatedDisputeQueryParameters,
                        splitOn: nameof(Dispute.CurrentStatus.DisputeStatusID)
                    )).SingleOrDefault();

                    await transaction.CommitAsync();       
                    return createdDispute;
                }
            }
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    public async Task<DisputeHistoryEntry?> CreateDisputeStatusHistoryEntryAsync(int disputeID, int newStatusID, int updatedByID, NpgsqlTransaction? tx)
    {
        string insertQuery = $@"
            INSERT INTO dispute_status_history (dispute_id, dispute_status_id, updated_at, updated_by_id)
            VALUES (@disputeID, @newStatusID, NOW(), @updatedByID)
            RETURNING dispute_history_id;
        ";

        string getCreatedDisputeHistoryQuery = $@"
            SELECT 
                dsh.dispute_history_id AS { nameof(DisputeHistoryEntry.DisputeHistoryID) },
                dsh.dispute_id AS { nameof(DisputeHistoryEntry.DisputeID) },
                dsh.updated_at AS { nameof(DisputeHistoryEntry.UpdatedAt) },
                dsh.dispute_status_id AS { nameof(DisputeHistoryEntry.Status.DisputeStatusID) },
                dsh.updated_by_id AS { nameof(DisputeHistoryEntry.UpdatedBy.UserID) },
                u.username AS { nameof(DisputeHistoryEntry.UpdatedBy.Username) },
                u.email AS { nameof(DisputeHistoryEntry.UpdatedBy.Email) },
                ds.dispute_status_id AS { nameof(DisputeHistoryEntry.Status.DisputeStatusID) },
                ds.name AS { nameof(DisputeHistoryEntry.Status.Name) }
            FROM dispute_status_history dsh
                INNER JOIN dispute_statuses ds ON dsh.dispute_status_id = ds.dispute_status_id
                INNER JOIN users u ON dsh.updated_by_id = u.user_id
            WHERE dsh.dispute_history_id = @disputeHistoryID;
        ";

        var parameters = new
        {
            disputeID,
            newStatusID,
            updatedByID
        };

        using var connection = new NpgsqlConnection(Constants.ConnectionString);
        await connection.OpenAsync();
        var transaction = tx ?? await connection.BeginTransactionAsync();

        try
        {
            int disputeHistoryID = await connection.ExecuteScalarAsync<int>(insertQuery, param: parameters, transaction: transaction);

            if (disputeHistoryID == 0)
            {
                if (tx == null) await transaction.RollbackAsync();
                return null;
            }
            else
            {
                var getCreatedDisputeHistoryParameters = new
                {
                    disputeHistoryID
                };

                var createdDisputeHistoryEntry = (await connection.QueryAsync<DisputeHistoryEntry, RedactedUser, DisputeStatus, DisputeHistoryEntry>(
                    getCreatedDisputeHistoryQuery,
                    _disputeHistoryEntryMapper,
                    param: getCreatedDisputeHistoryParameters,
                    splitOn: string.Join(',', nameof(DisputeHistoryEntry.UpdatedBy.UserID), nameof(DisputeHistoryEntry.Status.DisputeStatusID)),
                    transaction: transaction
                )).SingleOrDefault();

                if (tx == null)
                {
                    transaction.Commit();
                }
                else
                {
                    // this transaction should be handled by the function that created the transaction
                }
                return createdDisputeHistoryEntry;
            }
        }
        catch
        {
            if (tx == null)
            {
                await transaction.RollbackAsync();
            }
            else
            {
                // this transaction should be handled by the function that created the transaction
            }
            throw;
        }
        finally
        {
            if (tx == null)
            {
                await connection.CloseAsync();
            }
            else
            {
                // this transaction should be handled by the function that created the transaction
            }
        }

    }

    public async Task<IEnumerable<Dispute>> GetAllDisputesAsync(Pagination pagination, int? userId = null, string? status = null, string? email = null)
    {
        string query = $@"
            WITH dispute_status_history_with_to_date AS (
                SELECT 
                    dispute_history_id,
                    dispute_id,
                    dispute_status_id,
                    updated_at AS from_date,
                    LEAD(updated_at, 1, '9999-12-31') OVER (PARTITION BY dispute_id ORDER BY updated_at ASC) AS to_date
                FROM dispute_status_history
            ),
            dispute_with_current_status_and_user AS (
                SELECT
                    d.dispute_id,
                    d.reason,
                    d.disputed_transaction_reference_id,
                    d.created_at,
                    dsh.dispute_status_id,
                    ds.name,
                    u.user_id,
                    u.email
                FROM disputes d
                INNER JOIN dispute_status_history_with_to_date dsh ON d.dispute_id = dsh.dispute_id
                INNER JOIN dispute_statuses ds ON dsh.dispute_status_id = ds.dispute_status_id
                INNER JOIN transactions t ON d.disputed_transaction_reference_id = t.transaction_reference_id
                INNER JOIN accounts a ON t.account_id = a.account_id
                INNER JOIN users u ON a.user_id = u.user_id 
                WHERE NOW() BETWEEN dsh.from_date AND dsh.to_date
            )
            SELECT DISTINCT
                dwcs.dispute_id AS { nameof(Dispute.DisputeID) },
                dwcs.reason { nameof(Dispute.Reason) },
                dwcs.disputed_transaction_reference_id { nameof(Dispute.DisputedTransactionReferenceID) },
                dwcs.created_at { nameof(Dispute.CreatedAt) },
                dwcs.dispute_status_id { nameof(Dispute.CurrentStatus.DisputeStatusID) },
                dwcs.name { nameof(Dispute.CurrentStatus.Name) }
            FROM dispute_with_current_status_and_user dwcs
            WHERE 
                ((@userId is NULL OR dwcs.user_id = @userId) AND (@email is NULL OR dwcs.email = @email))
                AND (@status is NULL OR dwcs.name = @status)
            LIMIT @limit
            OFFSET @offset
        ";

        var parameters = new
        {
            limit = pagination.Limit,
            offset = pagination.Offset,
            userId,
            status,
            email
        };

        using var connection = new NpgsqlConnection(Constants.ConnectionString);
        return await connection.QueryAsync<Dispute, DisputeStatus, Dispute>(
            query,
            _disputeMapper,
            splitOn: nameof(Dispute.CurrentStatus.DisputeStatusID),
            param: parameters
        );
    }

    public async Task<Dispute?> GetDisputeAsync(int disputeId, int? userId = null)
    {
        string query = $@"
            SELECT 
                dwcs.dispute_id AS { nameof(Dispute.DisputeID) },
                dwcs.reason { nameof(Dispute.Reason) },
                dwcs.disputed_transaction_reference_id { nameof(Dispute.DisputedTransactionReferenceID) },
                dwcs.created_at { nameof(Dispute.CreatedAt) },
                dwcs.dispute_status_id { nameof(Dispute.CurrentStatus.DisputeStatusID) },
                dwcs.name { nameof(Dispute.CurrentStatus.Name) }
            FROM dispute_with_current_status dwcs
            WHERE dwcs.dispute_id = @disputeId AND (@userId is NULL OR dwcs.involved_user_id = @userId)
            LIMIT 1
        ";

        var parameters = new
        {
            disputeId,
            userId
        };

        using var connection = new NpgsqlConnection(Constants.ConnectionString);
        return (await connection.QueryAsync<Dispute, DisputeStatus, Dispute>(
            query,
            _disputeMapper,
            param: parameters,
            splitOn: nameof(Dispute.CurrentStatus.DisputeStatusID)
        )).SingleOrDefault();
    }

    public async Task<IEnumerable<DisputeHistoryEntry>> GetDisputeHistoryAsync(Pagination pagination, int disputeId, int? userId = null)
    {
        string query = $@"
            SELECT
                DISTINCT
                dsh.dispute_history_id AS { nameof(DisputeHistoryEntry.DisputeHistoryID) },
                dsh.dispute_id AS { nameof(DisputeHistoryEntry.DisputeID) },
                dsh.updated_at AS { nameof(DisputeHistoryEntry.UpdatedAt) },
                dsh.updated_by_id AS { nameof(DisputeHistoryEntry.UpdatedBy.UserID) },
                u.username AS { nameof(DisputeHistoryEntry.UpdatedBy.Username) },
                u.email AS { nameof(DisputeHistoryEntry.UpdatedBy.Email) },
                dsh.dispute_status_id AS { nameof(DisputeHistoryEntry.Status.DisputeStatusID) },
                ds.name AS { nameof(DisputeHistoryEntry.Status.Name) }
            FROM dispute_status_history dsh
                INNER JOIN users u ON dsh.updated_by_id = u.user_id
                INNER JOIN dispute_statuses ds ON dsh.dispute_status_id = ds.dispute_status_id
                INNER JOIN disputes_for_users dfu ON dfu.dispute_id = dsh.dispute_id
            WHERE dsh.dispute_id = @disputeId
                 AND (@userId IS NULL OR dfu.user_id = @userId)
            ORDER BY dsh.updated_at
            LIMIT @limit
            OFFSET @offset
        ";

        var parameters = new
        {
            limit = pagination.Limit,
            offset = pagination.Offset,
            disputeId,
            userId
        };

        using var connection = new NpgsqlConnection(Constants.ConnectionString);
        return await connection.QueryAsync<DisputeHistoryEntry, RedactedUser, DisputeStatus, DisputeHistoryEntry>(
            query,
            _disputeHistoryEntryMapper,
            splitOn: string.Join(',', nameof(DisputeHistoryEntry.UpdatedBy.UserID), nameof(DisputeHistoryEntry.Status.DisputeStatusID)),
            param: parameters
        );
    }

    public async Task<IEnumerable<DisputeStatus>> GetAllowedNextStatusesAsync(int disputeId)
    {
        var query = $@"
            SELECT DISTINCT
                adp.to_dispute_status_id AS { nameof(DisputeStatus.DisputeStatusID) },
                ds.name AS { nameof(DisputeStatus.Name) }
            FROM allowed_dispute_progressions adp
            INNER JOIN dispute_statuses ds ON adp.to_dispute_status_id = ds.dispute_status_id
            INNER JOIN dispute_with_current_status dwcs ON dwcs.dispute_status_id = adp.from_dispute_status_id
            WHERE dwcs.dispute_id = @disputeId
        ";

        var parameters = new
        {
            disputeId,
        };

        using var connection = new NpgsqlConnection(Constants.ConnectionString);
        var result = await connection.QueryAsync<DisputeStatus>(
            query,
            param: parameters
        );

        return result;
    }

    public async Task<bool> IsDisputeProgressionAllowedAsync(int disputeID, int newStatusID)
    {
        string query = $@"
            SELECT 1
            FROM allowed_dispute_progressions adp
            INNER JOIN dispute_with_current_status dwcs ON adp.from_dispute_status_id = dwcs.dispute_status_id
            WHERE adp.to_dispute_status_id = @newStatusID
                AND dwcs.dispute_id = @disputeID
        ";

        var parameters = new
        {
            disputeID,
            newStatusID
        };

        using var connection = new NpgsqlConnection(Constants.ConnectionString);
        var result = await connection.ExecuteScalarAsync<int?>(
            query,
            param: parameters
        );

        return result.HasValue;

    }
  }
}