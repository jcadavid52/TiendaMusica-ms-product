using TiendaMusica.Domain.Models.Result;
namespace TiendaMusica.Domain.Models
{
    public class Instrument:Entity<string>
    {
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public InstrumentType Type { get; private set; }
        public decimal Price { get; private set; }
        public int Stock { get; private set; }

        public Instrument(
            string name,
            string description,
            InstrumentType type,
            decimal price,
            int stock
            )
        {
            ArgumentOutOfRangeException.ThrowIfNegative(stock);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(price);
            ArgumentNullException.ThrowIfNull(stock, nameof(stock));
            ArgumentNullException.ThrowIfNull(type, nameof(type));
            ArgumentNullException.ThrowIfNull(type, nameof(price));
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentException.ThrowIfNullOrWhiteSpace(description);
            
            Name = name;
            Description = description;
            Type = type;
            Price = price;
            Stock = stock;
        }

        public Results<Instrument> ValidateExistenceByName(string name)
        {
            var results = new Results<Instrument>();

            if(name == Name)
            {
                results.AddError(ErrorCode.CONFLICT_ERROR, $"Ya hay un instrumento con el nombre '{name}'");
            }
            
            return results;
        }
    }
}
