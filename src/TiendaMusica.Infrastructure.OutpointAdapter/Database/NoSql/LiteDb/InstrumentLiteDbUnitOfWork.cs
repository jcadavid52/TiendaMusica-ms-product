using Polly;
using TiendaMusica.Domain.Models.Result;
using TiendaMusica.Domain.Ports;

namespace TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb
{
    public class InstrumentLiteDbUnitOfWork : IUnitOfWork
    {
        private readonly IMessagePublisherPort _messagePublisherPort;
        private readonly InstrumentLiteDbContext _context;
        private readonly IAsyncPolicy _circuitBreakerPolicy;

        public InstrumentLiteDbUnitOfWork(
            IMessagePublisherPort messagePublisherPort,
            InstrumentLiteDbContext context,
            IAsyncPolicy circuitBreakerPolicy)
        {
            _messagePublisherPort = messagePublisherPort;
            _context = context;
            _circuitBreakerPolicy = circuitBreakerPolicy;
        }

        public async Task<Results<bool>> SaveChangesAsync<TId>(CancellationToken cancellationToken = default)
        {
            var results = new Results<bool>();

            return await _circuitBreakerPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    var aggregateRoots = _context.GetTrackedEntities<TId>().ToList();

                    foreach (var root in aggregateRoots)
                    {
                        foreach (var @event in root.DomainEvents)
                        {
                            var publishResult = await _messagePublisherPort.PublishAsync(@event);

                            if (publishResult.HasErrors || !publishResult.Result)
                            {
                                results.AddErrors(publishResult.Errors);
                                results.Result = false;
                                return results;
                            }
                        }

                        root.ClearEvents();
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
                    return results;
                }
            });
        }
    }
}
