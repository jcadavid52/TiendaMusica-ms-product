namespace TiendaMusica.Domain.Models
{
    public abstract class Product : AggregateRoot<string>
    {
        public string Name { get; protected set; } = string.Empty;
        public string Description { get; protected set; } = string.Empty;
        public decimal Price { get; protected set; }
        public int Stock { get; protected set; }
        public int CategoryId { get; protected set; }
        public Category Category { get; protected set; } = null!;

        protected const decimal ShippingCost = 100;

        protected Product(string id,
            DateTime createdAt,
            string name,
            string description,
            decimal price,
            int stock,
            int categoryId)
            : base(id, createdAt)
        {
            ValidateRequiredFields(name, description, price, stock, categoryId);

            Name = name;
            Description = description;
            Price = price;
            Stock = stock;
            CategoryId = categoryId;
        }

        protected static bool ValidateMinimalPrice(decimal price)
        {
            return price < ShippingCost;
        }

        protected static void ValidateRequiredFields(
            string name,
            string description,
            decimal price,
            int stock,
            int categoryId)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(stock);
            ArgumentNullException.ThrowIfNull(stock, nameof(stock));
            ArgumentNullException.ThrowIfNull(price, nameof(price));
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentException.ThrowIfNullOrWhiteSpace(description);
            ArgumentOutOfRangeException.ThrowIfNegative(categoryId);
        }
    }
}
