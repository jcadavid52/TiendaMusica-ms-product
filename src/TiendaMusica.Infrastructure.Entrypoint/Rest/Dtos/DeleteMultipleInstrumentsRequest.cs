namespace TiendaMusica.Infrastructure.Entrypoint.Rest.Dtos
{
    public record DeleteMultipleInstrumentsRequest(IList<string> InstrumentIds);
}
