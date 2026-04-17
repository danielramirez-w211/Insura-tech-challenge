using InsuraTech.Domain.Common;
using MongoDB.Driver;

namespace InsuraTech.Infrastructure.Persistence.Repositories.Base;

public abstract class MongoRepository<T> where T : Entity
{
    protected readonly MongoDbContext Context;

    protected MongoRepository(MongoDbContext context) => Context = context;

    protected static FilterDefinition<T> NotDeleted()
        => Builders<T>.Filter.Eq(x => x.IsDeleted, false);

    protected static IFindFluent<T, T> ApplyPagination(IFindFluent<T, T> query, int page, int pageSize)
        => query.Skip((page - 1) * pageSize).Limit(pageSize);
}
