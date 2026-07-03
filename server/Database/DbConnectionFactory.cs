using System.Data;
using Npgsql;

public class DbConnectionFactory
{
    private IConfiguration _configuration;

    public DbConnectionFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(
            _configuration.GetConnectionString("Postgres")
        );
    }
}
