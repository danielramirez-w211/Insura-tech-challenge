using InsuraTech.Domain.Interfaces;
using InsuraTech.Domain.Policies;
using InsuraTech.Infrastructure.Persistence.Documents;
using MongoDB.Driver;

namespace InsuraTech.Infrastructure.Persistence.Repositories;

public sealed class CityRepository : ICityRepository
{
    private readonly MongoDbContext _context;

    public CityRepository(MongoDbContext context) => _context = context;

    public async Task<IEnumerable<CityInfo>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var documents = await _context.Cities
            .Find(Builders<CityDocument>.Filter.Empty)
            .SortBy(c => c.Name)
            .ToListAsync(cancellationToken);

        return documents.Select(d => new CityInfo(d.Name, d.PostalCode, d.Department));
    }
}
