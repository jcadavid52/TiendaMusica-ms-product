namespace TiendaMusica.Domain.Dtos
{
    public record InstrumentDto(
        string Id,
        string Name,
        string Description,
        decimal Price,
        int Stock
    );
}
