using System;
using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

public class DbConnectionFactory
{
    private readonly IConfiguration _configuration;

    public DbConnectionFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IDbConnection CreateConnection()
    {
        var connStr = _configuration.GetConnectionString("Postgres");
        
        // Fallback to DATABASE_URL environment variable commonly provided by Render / Heroku
        if (string.IsNullOrWhiteSpace(connStr) || connStr.Contains("YOUR_POSTGRES_PASSWORD"))
        {
            var envDatabaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            if (!string.IsNullOrWhiteSpace(envDatabaseUrl))
            {
                connStr = envDatabaseUrl;
            }
        }

        if (!string.IsNullOrWhiteSpace(connStr) && (connStr.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) || connStr.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)))
        {
            connStr = ParsePostgresUri(connStr);
        }

        return new NpgsqlConnection(connStr);
    }

    private static string ParsePostgresUri(string uriString)
    {
        var uri = new Uri(uriString);
        var userInfo = uri.UserInfo.Split(':', 2);
        var username = userInfo.Length > 0 ? userInfo[0] : "";
        var password = userInfo.Length > 1 ? userInfo[1] : "";
        var host = uri.Host;
        var port = uri.Port > 0 ? uri.Port : 5432;
        var database = uri.AbsolutePath.TrimStart('/');

        return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true;";
    }
}
