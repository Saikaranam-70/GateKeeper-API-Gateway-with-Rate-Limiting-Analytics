public interface IUserRepository
{
    Task<long> CreateAsync(User user);
    Task<User?> GetByEmailAsync(string email);
}