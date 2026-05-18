using TiendaMusica.Domain.Dtos;
using TiendaMusica.Domain.Models;

namespace TiendaMusica.Domain.Events
{
    public class InstrumentDeletedMassiveEvent : IDomainEvent
    {
        public InstrumentDeletedMassiveEvent(IEnumerable<Instrument> instruments)
        {
            Products = instruments.Select(instrument => new InstrumentDto(
                instrument.Id,
                instrument.Name,
                instrument.Description,
                instrument.Price,
                instrument.Stock
            )).ToList();
        }

        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
        public IEnumerable<InstrumentDto> Products { get; }
    }
}
