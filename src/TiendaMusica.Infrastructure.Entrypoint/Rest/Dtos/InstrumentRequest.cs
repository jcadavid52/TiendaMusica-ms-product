using TiendaMusica.Domain.Models;

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
