using System.Data;
using Api.Models;
using Dapper;

namespace Api.Repositories
{
    public interface ITransactionTypeRepository
    {
        public Task<IEnumerable<TransactionType>> GetTransactionTypesAsync();
    }

    public class TransactionTypeRepository : ITransactionTypeRepository
    {
        private readonly IDbConnection _dbConnection;
        public TransactionTypeRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }
        public async Task<IEnumerable<TransactionType>> GetTransactionTypesAsync()
        {
            string query = """
            SELECT 
                transaction_type_id AS "TransactionTypeId", 
                name AS "Name"
            FROM transaction_types;
        """;
            return await _dbConnection.QueryAsync<TransactionType>(query);
        }
    }
}