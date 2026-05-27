using TiendaMusica.Domain.Enums;

namespace TiendaMusica.Infrastructure.Entrypoint.Rest.Dtos
{
    public record InstrumentCreateRequest(
        string Name,
        string Description,
        InstrumentType Type,
        decimal Price,
        int Stock
    );
}
