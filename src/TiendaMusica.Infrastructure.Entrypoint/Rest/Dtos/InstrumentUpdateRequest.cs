using TiendaMusica.Domain.Enums;

namespace TiendaMusica.Infrastructure.Entrypoint.Rest.Dtos
{
    public record InstrumentUpdateRequest(
        string Id,
        string Name,
        string Description,
        InstrumentType Type
    );
}
