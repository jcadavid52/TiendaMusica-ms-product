namespace TiendaMusica.Application.Dtos
{
    public record DeleteMultipleInstrumentsCommand(IList<string> InstrumentIds);
}
