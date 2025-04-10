namespace Api.Services
{
    public class DBConnection
    {
        private readonly string _connectionString;
        public DBConnection(string connectionString)
        {
            _connectionString = connectionString;
        }
        
        public string DBConnectionString()
        {
            return _connectionString;
        }
    }
}
