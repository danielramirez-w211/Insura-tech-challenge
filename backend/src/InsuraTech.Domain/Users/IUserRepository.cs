namespace InsuraTech.Domain.Users;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool>  ExistsAnyAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetByLeaderIdAsync(Guid leaderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetAllLeadersAsync(CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task<string> GetNextAdvisorCodeAsync(CancellationToken cancellationToken = default);
}
