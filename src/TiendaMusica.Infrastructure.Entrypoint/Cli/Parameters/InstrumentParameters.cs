namespace TiendaMusica.Infrastructure.Entrypoint.Cli.Parameters
{
    public static class InstrumentParameters
    {
        public const string Help = "help";
        public const string Add = "instrument-add";
        public const string List = "instrument-list";
        public const string Delete = "instrument-delete-multiple";
        public const string GetById = "instrument-getbyid";
        public static string[] parameters = { Add, List, Delete, GetById, Help };
    }
}
