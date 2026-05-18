using System.Text.Json.Serialization;
using TiendaMusica.Domain.Enums;
using TiendaMusica.Domain.Events;
using TiendaMusica.Domain.Models.Result;
namespace TiendaMusica.Domain.Models
{
    public class Instrument : AggregateRoot<string>
    {
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public InstrumentType Type { get; private set; }
        public decimal Price { get; private set; }
        public int Stock { get; private set; }

        private const decimal ShippingCost = 100;

        [JsonConstructor]
        private Instrument(
            string name,
            string description,
            InstrumentType type,
            decimal price,
            int stock
            ) : base(Guid.NewGuid().ToString(), DateTime.UtcNow)
        {
            ValidateRequiredFields(name, description, type, price, stock);

            Name = name;
            Description = description;
            Type = type;
            Price = price;
            Stock = stock;
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

        public Results<Instrument> Update(
            string name,
            string description,
            InstrumentType type
            )
        {
            ArgumentNullException.ThrowIfNull(type, nameof(type));
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentException.ThrowIfNullOrWhiteSpace(description);

            var resultInstrument = new Results<Instrument>();

            if (!EnsureBundleConsistency(name, description))
                return resultInstrument.AddError(ErrorCode.VALIDATION_ERROR, "Bundles must have a description of at least 10 characters.");

            Name = name;
            Description = description;
            Type = type;

            resultInstrument.Result = this;

            return resultInstrument;
        }

        public static Results<Instrument> Create(string name, string description, InstrumentType type, decimal price, int stock)
        {
            var result = new Results<Instrument>();

            var validationError = ValidateBusinessRules(name, description, price);
            if (validationError != null) return result.AddError(ErrorCode.VALIDATION_ERROR, validationError);

            var instrument = new Instrument(name, description, type, price, stock);

            instrument.RaiseEvent(new InstrumentCreatedEvent(instrument));

            result.Result = instrument;
            return result;
        }

        private static string? ValidateBusinessRules(string name, string description, decimal price)
        {
            if (ValidateMinimalPrice(price))
                return "El precio no puede ser menor al costo de envío";

            bool isBundle = name.Contains("Pack", StringComparison.OrdinalIgnoreCase) ||
                            name.Contains("Kit", StringComparison.OrdinalIgnoreCase);

            if (isBundle && description.Length < 10)
                return "Bundles must have a description of at least 10 characters.";

            return null;
        }

        private static void ValidateRequiredFields(
            string name,
            string description,
            InstrumentType type,
            decimal price,
            int stock)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(stock);
            ArgumentNullException.ThrowIfNull(stock, nameof(stock));
            ArgumentNullException.ThrowIfNull(type, nameof(type));
            ArgumentNullException.ThrowIfNull(price, nameof(price));
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentException.ThrowIfNullOrWhiteSpace(description);
        }
    }
}
