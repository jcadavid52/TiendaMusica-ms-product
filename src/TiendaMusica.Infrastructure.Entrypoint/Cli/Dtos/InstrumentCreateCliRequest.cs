using TiendaMusica.Domain.Enums;

namespace TiendaMusica.Infrastructure.Entrypoint.Cli.Dtos
{
    public record InstrumentCreateCliRequest(
        string Name,
        string Description,
        InstrumentType Type,
        decimal Price,
        int Stock,
        int CategoryId
        );
}
