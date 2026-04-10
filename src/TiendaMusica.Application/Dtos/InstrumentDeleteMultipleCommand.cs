namespace TiendaMusica.Application.Dtos
{
    public record InstrumentDeleteMultipleCommand(IList<string> InstrumentIds);
}
