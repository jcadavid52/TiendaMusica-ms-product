namespace TiendaMusica.Infrastructure.Entrypoint.Cli.Parameters
{
    public static class InstrumentParameters
    {
        public const string Help = "help";
        public const string Add = "instrument-add";
        public const string List = "instrument-list";
        public const string Delete = "delete-multiple";
        public const string GetById = "";
        public static string[] parameters = { Add, List, Delete , Help };
    }
}
