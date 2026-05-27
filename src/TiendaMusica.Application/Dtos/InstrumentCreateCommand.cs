using TiendaMusica.Domain.Enums;

namespace TiendaMusica.Application.Dtos
{
    public class InstrumentCreateCommand
    {
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public InstrumentType Type { get; private set; }
        public decimal Price { get; private set; }
        public int Stock { get; private set; }
        public int CategoryId { get; private set; }

        public InstrumentCreateCommand(string name,
            string description,
            InstrumentType type,
            decimal price,
            int stock,
            int categoryId)
        {
            Name = name.Trim();
            Description = description.Trim();
            Type = type;
            Price = price;
            Stock = stock;
            CategoryId = categoryId;
        }
    }
}
