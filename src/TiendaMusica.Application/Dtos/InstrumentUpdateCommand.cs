using TiendaMusica.Domain.Enums;

namespace TiendaMusica.Application.Dtos
{
    public class InstrumentUpdateCommand
    {
        public string Id { get; private set; } = string.Empty;
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public InstrumentType Type { get; private set; }

        public InstrumentUpdateCommand(string id,
            string name,
            string description,
            InstrumentType type
            )
        {
            Id = id.Trim();
            Name = name.Trim();
            Description = description.Trim();
            Type = type;
        }
    }
}
