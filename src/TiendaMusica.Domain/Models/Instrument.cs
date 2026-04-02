using TiendaMusica.Domain.Enums;
using TiendaMusica.Domain.Models.Result;
namespace TiendaMusica.Domain.Models
{
    public class Instrument : Entity<string>
    {
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public InstrumentType Type { get; private set; }
        public decimal Price { get; private set; }
        public int Stock { get; private set; }

        private const decimal ShippingCost = 100;

        private Instrument(
            string name,
            string description,
            InstrumentType type,
            decimal price,
            int stock
            ) : base(Guid.NewGuid().ToString(), DateTime.UtcNow)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(stock);
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

        public static Results<Instrument> Create(
            string name,
            string description,
            InstrumentType type,
            decimal price,
            int stock
            )
        {
            var resultInstrument = new Results<Instrument>();

            if (ValidateMinimalPrice(price))
                return resultInstrument.AddError(ErrorCode.VALIDATION_ERROR, "El precio no puede ser menor al costo de envío");

            if (!EnsureBundleConsistency(name, description))
                return resultInstrument.AddError(ErrorCode.VALIDATION_ERROR, "Bundles must have a description of at least 10 characters.");

            resultInstrument.Result = new Instrument(name, description, type, price, stock);
            return resultInstrument;
        }

        private static bool EnsureBundleConsistency(string name, string description)
        {
            bool isBundle = name.Contains("Pack") || name.Contains("Kit");

            if (isBundle && description.Length < 10) return false;

            return true;
        }

        private static bool ValidateMinimalPrice(decimal price)
        {
            return price < ShippingCost;
        }
    }
}
