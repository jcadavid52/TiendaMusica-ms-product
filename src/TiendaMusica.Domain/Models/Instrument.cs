using System.Text.Json.Serialization;
using TiendaMusica.Domain.Enums;
using TiendaMusica.Domain.Events;
using TiendaMusica.Domain.Models.Result;
namespace TiendaMusica.Domain.Models
{
    public class Instrument : Product
    {
        public InstrumentType Type { get; private set; }

        [JsonConstructor]
        private Instrument(
            string name,
            string description,
            InstrumentType type,
            decimal price,
            int stock,
            int categoryId
            ) : base(Guid.NewGuid().ToString(), DateTime.UtcNow, name, description, price, stock, categoryId)
        {
            ValidateInstrumentRequiredFields(type);

            Type = type;
        }

        private static bool EnsureBundleConsistency(string name, string description)
        {
            bool isBundle = name.Contains("Pack") || name.Contains("Kit");

            if (isBundle && description.Length < 10) return false;

            return true;
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

        public static Results<Instrument> Create(
            string name,
            string description,
            InstrumentType type,
            decimal price,
            int stock,
            int categoryId)
        {
            var result = new Results<Instrument>();

            var validationError = ValidateBusinessRules(name, description, price);
            if (validationError != null) return result.AddError(ErrorCode.VALIDATION_ERROR, validationError);

            var instrument = new Instrument(name, description, type, price, stock,categoryId);

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

        private static void ValidateInstrumentRequiredFields(InstrumentType type)
        {
            ArgumentNullException.ThrowIfNull(type, nameof(type));
        }
    }
}
