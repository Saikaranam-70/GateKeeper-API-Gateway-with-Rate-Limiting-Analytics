using GateKeeper.Database;
using GateKeeper.Middleware;
using GateKeeper.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Enable snake_case mapping globally for Dapper
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

// Database Configuration
builder.Services.AddScoped<DbConnectionFactory>();
builder.Services.AddScoped<MigrationRunner>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IGatewayService, GatewayService>();
builder.Services.AddScoped<IGatewayRepository, GatewayRepository>();

// Redis Setup
var redisConnStr = builder.Configuration.GetConnectionString("Redis");
ConnectionMultiplexer redisMultiplexer;

// Handle both redis:// and rediss:// (SSL) URI schemes — Upstash uses rediss://
var isRedisUri = !string.IsNullOrEmpty(redisConnStr) &&
                 (redisConnStr.StartsWith("redis://", StringComparison.OrdinalIgnoreCase) ||
                  redisConnStr.StartsWith("rediss://", StringComparison.OrdinalIgnoreCase));

if (isRedisUri)
{
    var uri = new Uri(redisConnStr!);
    var host = uri.Host;
    var port = uri.Port > 0 ? uri.Port : 6379;
    var password = uri.UserInfo.Contains(':')
        ? uri.UserInfo.Split(':', 2)[1]
        : uri.UserInfo;

    var useSsl = redisConnStr!.StartsWith("rediss://", StringComparison.OrdinalIgnoreCase);

    var configOptions = new ConfigurationOptions
    {
        EndPoints = { { host, port } },
        Password = password,
        AbortOnConnectFail = false,
        Ssl = useSsl,
        SslProtocols = useSsl ? System.Security.Authentication.SslProtocols.Tls12 : System.Security.Authentication.SslProtocols.None
    };
    redisMultiplexer = ConnectionMultiplexer.Connect(configOptions);
}
else
{
    redisMultiplexer = ConnectionMultiplexer.Connect(redisConnStr ?? "localhost:6379");
}
builder.Services.AddSingleton<IConnectionMultiplexer>(redisMultiplexer);
builder.Services.AddScoped<ICacheService, RedisCacheService>();

// JWT Authentication
var jwtSecret = builder.Configuration["JwtSettings:Secret"]!;
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"]!;
var jwtAudience = builder.Configuration["JwtSettings:Audience"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecret)
            )
        };
    });

builder.Services.AddAuthorization();

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
app.UseAuthentication();
app.UseAuthorization();
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

