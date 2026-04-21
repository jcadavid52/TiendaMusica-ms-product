using TiendaMusica.Domain.Enums;

namespace TiendaMusica.Domain.Dtos
{
    public record InstrumentGetAllQueryParametersDto(
        string? Search,
        string? OrderBy,
        int? PageNumber,
        int? PageSize,
        SortDirection SortDirection = SortDirection.Desc);
}
