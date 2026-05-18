namespace TiendaMusica.Domain.Models
{
    public abstract class AggregateRoot<TId> : Entity<TId>
    {
        private readonly List<object> _domainEvents = new();
        public IReadOnlyCollection<object> DomainEvents => _domainEvents.AsReadOnly();

        protected void RaiseEvent(object domainEvent) => _domainEvents.Add(domainEvent);
        public void ClearEvents() => _domainEvents.Clear();

        protected AggregateRoot(TId id, DateTime createdAt) : base(id, createdAt) { }
    }
}
