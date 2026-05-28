namespace TiendaMusica.Domain.Models
{
    public class Category : AggregateRoot<int>
    {
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public ICollection<Product> Products { get; private set; }
        = new List<Product>();

        public Category(
            int id,
            string name,
            string description
            ) : base(id, DateTime.UtcNow)
        {
            ValidateRequiredFields(name, description);
            Name = name;
            Description = description;
        }

        protected static void ValidateRequiredFields(string name, string description)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentException.ThrowIfNullOrWhiteSpace(description);
        }
    }
}
