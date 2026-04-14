using TiendaMusica.Domain.Enums;

namespace TiendaMusica.Domain.Dtos
{
    public record InstrumentStockSummary(
        InstrumentType Type,
        int TotalStock
    );
}
