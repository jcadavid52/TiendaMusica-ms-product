using TiendaMusica.Domain.Models;

namespace TiendaMusica.Domain.Events
{
    public class InstrumentCreatedEvent : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();

        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public string InstrumentId { get; }
        public string Name { get; }
        public string Description { get; }
        public decimal Price { get; }
        public int Stock { get; }

        public InstrumentCreatedEvent(Instrument instrument)
        {
            InstrumentId = instrument.Id;
            Name = instrument.Name;
            Price = instrument.Price;
            Stock = instrument.Stock;
            Description = instrument.Description;
        }
    }
}
