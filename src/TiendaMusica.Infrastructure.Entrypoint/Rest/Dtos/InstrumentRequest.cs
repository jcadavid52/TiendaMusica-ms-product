using TiendaMusica.Domain.Enums;

namespace TiendaMusica.Infrastructure.Entrypoint.Rest.Dtos
{
    public record InstrumentRequest(
        string Name,
        string Description,
        InstrumentType Type,
        decimal Price,
        int Stock
    );
}
