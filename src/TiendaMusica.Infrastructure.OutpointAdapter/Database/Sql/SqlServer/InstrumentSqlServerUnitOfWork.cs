using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;
using TiendaMusica.Domain.Models;
using TiendaMusica.Domain.Models.Result;
using TiendaMusica.Domain.Ports;

namespace TiendaMusica.Infrastructure.OutpointAdapter.Database.Sql.SqlServer
{
    public class InstrumentSqlServerUnitOfWork : IUnitOfWork
    {
        private readonly InstrumentSqlServerDbContext _context;
        private readonly IMessagePublisherPort _messagePublisherPort;
        private readonly IAsyncPolicy _circuitBreakerPolicy;
        private readonly DomainEventsCollector _domainEventsCollector;
        private readonly ICachePort _cachePort;
        private readonly ILogger<InstrumentSqlServerUnitOfWork> _logger;

        public InstrumentSqlServerUnitOfWork(
            InstrumentSqlServerDbContext context,
            IMessagePublisherPort messagePublisherPort,
            IAsyncPolicy circuitBreakerPolicy,
            DomainEventsCollector domainEventsCollector,
            ICachePort cachePort,
            ILogger<InstrumentSqlServerUnitOfWork> logger)
        {
            _context = context;
            _messagePublisherPort = messagePublisherPort;
            _circuitBreakerPolicy = circuitBreakerPolicy;
            _domainEventsCollector = domainEventsCollector;
            _cachePort = cachePort;
            _logger = logger;
        }

        public async Task<Results<bool>> SaveChangesAsync<TId>(CancellationToken cancellationToken = default)
        {
            var results = new Results<bool>();

            return await _circuitBreakerPolicy.ExecuteAsync(async () =>
            {
                using var transaction = _context.Database.IsRelational()
                 ? await _context.Database.BeginTransactionAsync(cancellationToken)
                 : null;

                try
                {
                    int rowsAffected = await _context.SaveChangesAsync(cancellationToken);

                    var allEvents = new List<object>();

                    var aggregateRoots = _context.ChangeTracker.Entries()
                    .Where(e => e.Entity is AggregateRoot<TId> root && root.DomainEvents.Count != 0)
                    .Select(e => (AggregateRoot<TId>)e.Entity)
                    .ToList();

                    foreach (var root in aggregateRoots)
                    {
                        allEvents.AddRange(root.DomainEvents);
                    }

                    if (_domainEventsCollector.Events.Count != 0)
                    {
                        allEvents.AddRange(_domainEventsCollector.Events);
                    }

                    if (allEvents.Count != 0 && rowsAffected >= 1)
                    {
                        foreach (var @event in allEvents)
                        {
                            var publishResult = await _messagePublisherPort.PublishAsync(@event);

                            if (publishResult.HasErrors || !publishResult.Result)
                            {
                                if (transaction != null)
                                    await transaction.RollbackAsync(cancellationToken);

                                results.AddErrors(publishResult.Errors);
                                results.Result = false;
                                return results;
                            }
                        }

                        foreach (var root in aggregateRoots)
                        {
                            root.ClearEvents();
                        }

                        _domainEventsCollector.Clear();
                    }

                    if (transaction != null)
                    {
                        await transaction.CommitAsync(cancellationToken);
                    }

                    results.Result = true;

                    return results;
                }
                catch (Exception ex)
                {
                    if (transaction != null)
                        await transaction.RollbackAsync(cancellationToken);
                    results.AddError(ErrorCode.DATABASE_ERROR, $"An error occurred while saving changes: {ex.Message}");
                    results.Result = false;

                    _logger.LogError(ex, "An exception occurred while saving changes to the database. Transaction rolled back. Errors: {Errors}", string.Join(", ", results.Errors.Select(e => e.Message)));

                    return results;
                }
                finally
                {
                    var removeByPatternResult = await _cachePort.RemoveByPatternAsync("product:*");
                    if (removeByPatternResult.HasErrors)
                    {
                        _logger.LogWarning("Failed to remove cache entries with pattern 'product:*' after saving changes. Errors: {Errors}", string.Join(", ", removeByPatternResult.Errors.Select(e => e.Message)));
                    }
                }
            });
        }
    }
}
