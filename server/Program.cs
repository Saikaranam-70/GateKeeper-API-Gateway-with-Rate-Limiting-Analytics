using GateKeeper.Database;
using GateKeeper.Middleware;
using GateKeeper.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Enable snake_case mapping globally for Dapper
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

// Database Configuration
builder.Services.AddScoped<DbConnectionFactory>();
builder.Services.AddScoped<MigrationRunner>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IJwtService, JwtService>();

// Redis Setup
var redisConnStr = builder.Configuration.GetConnectionString("Redis");
ConnectionMultiplexer redisMultiplexer;
if (!string.IsNullOrEmpty(redisConnStr) && redisConnStr.StartsWith("redis://", StringComparison.OrdinalIgnoreCase))
{
    var uri = new Uri(redisConnStr);
    var host = uri.Host;
    var port = uri.Port;
    var password = uri.UserInfo.Split(':').LastOrDefault();

    var configOptions = new ConfigurationOptions
    {
        EndPoints = { { host, port } },
        Password = password,
        AbortOnConnectFail = false,
        Ssl = true // Upstash endpoints require SSL/TLS negotiation, even when using the redis:// scheme on port 6379.
    };
    redisMultiplexer = ConnectionMultiplexer.Connect(configOptions);
}
else
{
    redisMultiplexer = ConnectionMultiplexer.Connect(redisConnStr ?? "localhost:6379");
}
builder.Services.AddSingleton<IConnectionMultiplexer>(redisMultiplexer);
builder.Services.AddScoped<ICacheService, RedisCacheService>();

// Controllers & OpenAPI (Swagger)
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Global Exception Handler Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Enable OpenAPI routes (endpoints like /openapi/v1.json)
app.MapOpenApi();

// Enable routing and controller mapping
app.UseRouting();
app.MapControllers();

// Run migrations on startup
using (var scope = app.Services.CreateScope())
{
    var migrationRunner = scope.ServiceProvider.GetRequiredService<MigrationRunner>();
    try
    {
        await migrationRunner.RunMigrationsAsync();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while running database migrations.");
        throw;
    }
}

app.Run();

