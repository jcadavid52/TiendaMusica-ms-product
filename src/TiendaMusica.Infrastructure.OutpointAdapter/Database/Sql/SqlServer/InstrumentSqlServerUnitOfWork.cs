using Microsoft.EntityFrameworkCore;
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

        public InstrumentSqlServerUnitOfWork(
            InstrumentSqlServerDbContext context,
            IMessagePublisherPort messagePublisherPort,
            IAsyncPolicy circuitBreakerPolicy,
            DomainEventsCollector domainEventsCollector)
        {
            _context = context;
            _messagePublisherPort = messagePublisherPort;
            _circuitBreakerPolicy = circuitBreakerPolicy;
            _domainEventsCollector = domainEventsCollector;
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
                    .Where(e => e.Entity is AggregateRoot<TId> root && root.DomainEvents.Any())
                    .Select(e => (AggregateRoot<TId>)e.Entity)
                    .ToList();

                    foreach (var root in aggregateRoots)
                    {
                        allEvents.AddRange(root.DomainEvents);
                    }

                    if (_domainEventsCollector.Events.Any())
                    {
                        allEvents.AddRange(_domainEventsCollector.Events);
                    }

                    if (allEvents.Any() && rowsAffected >= 1)
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
                    return results;
                }
            });
        }
    }
}
