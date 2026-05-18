using TiendaMusica.Domain.Dtos;
using TiendaMusica.Domain.Models;

namespace TiendaMusica.Domain.Events
{
    public class InstrumentCreatedEvent : IDomainEvent
    {
        public InstrumentCreatedEvent(Instrument instrument)
        {
            Product = new InstrumentDto(
                instrument.Id,
                instrument.Name,
                instrument.Description,
                instrument.Price,
                instrument.Stock
            );
        }

        public Guid EventId { get; } = Guid.NewGuid();

        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public InstrumentDto Product { get; }
    }
}
