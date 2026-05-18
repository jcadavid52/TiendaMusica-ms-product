namespace TiendaMusica.Domain.Models
{
    public class DomainEventsCollector
    {
        private readonly List<object> _events = new();
        public IReadOnlyCollection<object> Events => _events.AsReadOnly();

        public void AddEvent(object domainEvent) => _events.Add(domainEvent);
        public void Clear() => _events.Clear();
    }
}
