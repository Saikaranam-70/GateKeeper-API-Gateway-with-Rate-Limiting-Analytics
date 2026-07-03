using Dapper;

public class UserRepository : IUserRepository
{

    private readonly DbConnectionFactory _factory;

    public UserRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }
    public async Task<long> CreateAsync(User user)
    {
        using var connection = _factory.CreateConnection();
        var sql = @"INSERT INTO users (name, email, password_hash, role, uid) VALUES (@Name, @Email, @PasswordHash, @Role, @Uid) RETURNING id";

        return await connection.ExecuteScalarAsync<long>(sql, user);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        using var connection = _factory.CreateConnection();

        var sql = @"
        SELECT * FROM users WHERE email=@Email";

        return await connection.QueryFirstOrDefaultAsync<User>(
            sql, new {Email = email}
        );
    }
}