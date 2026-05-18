using TiendaMusica.Domain.Enums;

namespace TiendaMusica.Application.Dtos
{
    public record InstrumentGetAllQuery(
        SortDirection SortDirection = SortDirection.Desc,
        string? Search = "",
        int? PageSize = 10,
        int? PageNumber = 1
        );
}
