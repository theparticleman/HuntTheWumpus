namespace HuntTheWumpus.Infrastructure.Repositories;

using System.Data;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

public interface IDatabaseConnection
{
    IDbConnection CreateConnection();
}

public class MySqlDatabaseConnection : IDatabaseConnection
{
    private readonly string _connectionString;
    
    public MySqlDatabaseConnection(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentNullException("DefaultConnection string is not configured");
    }
    
    public IDbConnection CreateConnection()
    {
        return new MySqlConnection(_connectionString);
    }
}
