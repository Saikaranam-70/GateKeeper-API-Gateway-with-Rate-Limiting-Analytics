public interface IUserRepository
{
    Task<Guid> CreateAsync(User user);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);
    Task UpdateAsync(User user);
}