using InsuraTech.Domain.Policies;

namespace InsuraTech.Domain.Interfaces
{
    public interface ICityRepository
    {
        Task<IEnumerable<CityInfo>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
