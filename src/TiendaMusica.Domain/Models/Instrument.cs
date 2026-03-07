using TiendaMusica.Domain.Models.Result;

namespace TiendaMusica.Domain.Models
{
    public class Instrument:Entity<string>
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public InstrumentType Type { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }

        public Results<Instrument> ValidateExistenceByName(string name)
        {
            var results = new Results<Instrument>();

            if(name == Name)
            {
                results.AddError(ErrorCode.CONFLICT_ERROR, $"Ya hay un instrumento con el nombre '{name}'");
            }
            
            return results;
        }
    }
}
