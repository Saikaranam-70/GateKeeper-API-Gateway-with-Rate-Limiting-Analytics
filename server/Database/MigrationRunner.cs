using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GateKeeper.Database
{
    public class MigrationRunner
    {
        private readonly DbConnectionFactory _dbConnectionFactory;
        private readonly IHostEnvironment _environment;
        private readonly ILogger<MigrationRunner> _logger;

        public MigrationRunner(
            DbConnectionFactory dbConnectionFactory,
            IHostEnvironment environment,
            ILogger<MigrationRunner> logger)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _environment = environment;
            _logger = logger;
        }

        public async Task RunMigrationsAsync()
        {
            _logger.LogInformation("Starting database migration runner...");

            using var connection = _dbConnectionFactory.CreateConnection();
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            // Ensure schema_version table exists
            await connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS schema_version (
                    version VARCHAR(100) PRIMARY KEY,
                    applied_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
                );
            ");

            // Locate migration files
            var migrationsFolder = Path.Combine(_environment.ContentRootPath, "Database", "Migrations");
            if (!Directory.Exists(migrationsFolder))
            {
                // Fallback for different build/run environments
                migrationsFolder = Path.Combine(AppContext.BaseDirectory, "Database", "Migrations");
                if (!Directory.Exists(migrationsFolder))
                {
                    _logger.LogWarning($"Migrations directory not found at content root or base directory. Path attempted: {migrationsFolder}");
                    return;
                }
            }

            var migrationFiles = Directory.GetFiles(migrationsFolder, "V*__*.sql")
                                          .OrderBy(f => Path.GetFileName(f))
                                          .ToList();

            if (!migrationFiles.Any())
            {
                _logger.LogInformation("No migration files found.");
                return;
            }

            // Fetch already applied migrations
            var appliedMigrations = (await connection.QueryAsync<string>(
                "SELECT version FROM schema_version"
            )).ToHashSet();

            foreach (var filePath in migrationFiles)
            {
                var fileName = Path.GetFileName(filePath);
                if (appliedMigrations.Contains(fileName))
                {
                    _logger.LogInformation($"Migration {fileName} is already applied.");
                    continue;
                }

                _logger.LogInformation($"Applying migration: {fileName}...");

                var sql = await File.ReadAllTextAsync(filePath);

                using var transaction = connection.BeginTransaction();
                try
                {
                    // Execute migration SQL script
                    await connection.ExecuteAsync(sql, transaction: transaction);

                    // Record applied migration
                    await connection.ExecuteAsync(
                        "INSERT INTO schema_version (version) VALUES (@Version)",
                        new { Version = fileName },
                        transaction: transaction
                    );

                    transaction.Commit();
                    _logger.LogInformation($"Successfully applied migration: {fileName}");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _logger.LogError(ex, $"Failed to apply migration: {fileName}. Transaction rolled back.");
                    throw;
                }
            }

            _logger.LogInformation("Database migration runner completed successfully.");
        }
    }
}
