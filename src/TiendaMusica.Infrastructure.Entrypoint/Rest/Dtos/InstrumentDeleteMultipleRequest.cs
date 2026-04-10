namespace TiendaMusica.Infrastructure.Entrypoint.Rest.Dtos
{
    public record InstrumentDeleteMultipleRequest(IList<string> InstrumentIds);
}
