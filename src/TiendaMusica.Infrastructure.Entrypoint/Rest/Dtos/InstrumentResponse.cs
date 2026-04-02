using TiendaMusica.Domain.Enums;

namespace TiendaMusica.Infrastructure.Entrypoint.Rest.Dtos
{
    public record InstrumentResponse(
        string Id,
        string Name,
        string Description,
        InstrumentType Type,
        decimal Price,
        int Stock
    )
    {
        public string CreationDateUtc { get; init; } = string.Empty;
    }
}
