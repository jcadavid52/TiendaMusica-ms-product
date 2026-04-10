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

        public InstrumentSqlServerUnitOfWork(
            InstrumentSqlServerDbContext context,
            IMessagePublisherPort messagePublisherPort,
            IAsyncPolicy circuitBreakerPolicy)
        {
            _context = context;
            _messagePublisherPort = messagePublisherPort;
            _circuitBreakerPolicy = circuitBreakerPolicy;
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

                    var aggregateRoots = _context.ChangeTracker.Entries()
                    .Where(e => e.Entity is AggregateRoot<TId> root && root.DomainEvents.Any())
                    .Select(e => (AggregateRoot<TId>)e.Entity)
                    .ToList();

                    if (aggregateRoots.Any() && rowsAffected >= 1)
                    {
                        foreach (var root in aggregateRoots)
                        {
                            foreach (var @event in root.DomainEvents)
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

                            root.ClearEvents();
                        }
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
