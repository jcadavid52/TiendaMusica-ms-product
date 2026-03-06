using TiendaMusica.Domain.Models;

namespace TiendaMusica.Infrastructure.Entrypoint.Rest.Dtos
{
    public record InstrumentResponse(
        string Id,
        string Name,
        string Description,
        InstrumentType Type,
        decimal Price,
        int Stock
        );
}
