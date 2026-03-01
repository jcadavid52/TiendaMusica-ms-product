namespace TiendaMusica.Domain.Models
{
    public class Entity<T>
    {
        public T Id { get; set; }
        public DateTime CreationDateUtc { get; set; }
    }
}
