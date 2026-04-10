using TiendaMusica.Domain.Enums;

namespace TiendaMusica.Infrastructure.Entrypoint.Cli.Dtos
{
    public record InstrumentUpdateCliRequest(
        string Id,
        string Name,
        string Description,
        InstrumentType Type
        );
}
