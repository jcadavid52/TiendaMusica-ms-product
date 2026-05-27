using Microsoft.Extensions.Logging;
using Polly;
using TiendaMusica.Domain.Models;
using TiendaMusica.Domain.Models.Result;
using TiendaMusica.Domain.Ports;

namespace TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb
{
    public class InstrumentLiteDbUnitOfWork : IUnitOfWork
    {
        private readonly IMessagePublisherPort _messagePublisherPort;
        private readonly InstrumentLiteDbContext _context;
        private readonly IAsyncPolicy _circuitBreakerPolicy;
        private readonly DomainEventsCollector _domainEventsCollector;
        private readonly ILogger<InstrumentLiteDbUnitOfWork> _logger;

        public InstrumentLiteDbUnitOfWork(
            IMessagePublisherPort messagePublisherPort,
            InstrumentLiteDbContext context,
            IAsyncPolicy circuitBreakerPolicy,
            DomainEventsCollector domainEventsCollector,
            ILogger<InstrumentLiteDbUnitOfWork> logger
            )
        {
            _messagePublisherPort = messagePublisherPort;
            _context = context;
            _circuitBreakerPolicy = circuitBreakerPolicy;
            _domainEventsCollector = domainEventsCollector;
            _logger = logger;
        }

        public async Task<Results<bool>> SaveChangesAsync<TId>(CancellationToken cancellationToken = default)
        {
            var results = new Results<bool>();

            return await _circuitBreakerPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    var allEvents = new List<object>();
                    var aggregateRoots = _context.GetTrackedEntities<TId>().ToList();

                    foreach (var root in aggregateRoots)
                    {
                        allEvents.AddRange(root.DomainEvents);
                    }

                    if (_domainEventsCollector.Events.Any())
                    {
                        allEvents.AddRange(_domainEventsCollector.Events);
                    }

                    if (allEvents.Any())
                    {
                        foreach (var @event in allEvents)
                        {
                            var publishResult = await _messagePublisherPort.PublishAsync(@event);

                            if (publishResult.HasErrors || !publishResult.Result)
                            {
                                _context.Context.Rollback();
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

                    _context.ClearTracker();
                    results.Result = true;
                    return results;
                }
                catch (Exception ex)
                {
                    _context.Context.Rollback();
                    results.AddError(ErrorCode.DATABASE_ERROR, $"An error occurred while saving changes: {ex.Message}");
                    results.Result = false;

                    _logger.LogError(ex, "An exception occurred while saving changes to the database. Transaction rolled back. Errors: {Errors}", string.Join(", ", results.Errors.Select(e => e.Message)));

                    return results;
                }
            });
        }
    }
}
