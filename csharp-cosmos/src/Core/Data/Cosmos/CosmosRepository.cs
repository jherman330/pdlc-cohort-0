using System.Linq;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Todo.Core.Common.Entities;
using Todo.Core.Exceptions;

namespace Todo.Core.Data.Cosmos;

/// <summary>
/// Cosmos DB implementation of IRepository using the Cosmos SDK. Uses entity Id as partition key.
/// </summary>
public sealed class CosmosRepository<T> : BaseRepository<T> where T : BaseEntity
{
    private readonly ICosmosDbContext _context;

    public CosmosRepository(ICosmosDbContext context, ILogger<CosmosRepository<T>> logger)
        : base(logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    private Container Container => _context.GetContainer<T>();

    protected override async Task<T?> GetByIdCoreAsync(string id, CancellationToken cancellationToken)
    {
        try
        {
            var response = await Container.ReadItemAsync<T>(id, new PartitionKey(id), cancellationToken: cancellationToken).ConfigureAwait(false);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    protected override async Task<IReadOnlyList<T>> GetAllCoreAsync(CancellationToken cancellationToken)
    {
        var query = new QueryDefinition("SELECT * FROM c");
        using var iterator = Container.GetItemQueryIterator<T>(query);
        var list = new List<T>();
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken).ConfigureAwait(false);
            list.AddRange(response);
        }
        return list;
    }

    protected override async Task<T> CreateCoreAsync(T entity, CancellationToken cancellationToken)
    {
        var response = await Container.CreateItemAsync(entity, new PartitionKey(entity.Id), cancellationToken: cancellationToken).ConfigureAwait(false);
        return response.Resource;
    }

    protected override async Task<T> UpdateCoreAsync(T entity, CancellationToken cancellationToken)
    {
        var response = await Container.ReplaceItemAsync(entity, entity.Id, new PartitionKey(entity.Id), cancellationToken: cancellationToken).ConfigureAwait(false);
        return response.Resource;
    }

    protected override async Task DeleteCoreAsync(string id, CancellationToken cancellationToken)
    {
        try
        {
            await Container.DeleteItemAsync<T>(id, new PartitionKey(id), cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new EntityNotFoundException(typeof(T).Name, id, ex);
        }
    }

    /// <inheritdoc />
    /// <remarks>Cosmos does not expose LINQ IQueryable over the wire. Use GetAllAsync or add Cosmos-specific query methods.</remarks>
    public override IQueryable<T> Query() => Enumerable.Empty<T>().AsQueryable();

    protected override void MapAndThrow(Exception ex, string operation, string? id = null)
    {
        if (ex is CosmosException cosmosEx)
        {
            if (cosmosEx.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new EntityNotFoundException(typeof(T).Name, id ?? string.Empty, cosmosEx);
            throw new DataAccessException(operation, id, cosmosEx.Message, cosmosEx.StatusCode, cosmosEx);
        }
        throw new DataAccessException(operation, id, ex.Message, System.Net.HttpStatusCode.InternalServerError, ex);
    }
}
