using TiendaMusica.Domain.Models;

namespace TiendaMusica.Infrastructure.OutpointAdapter.Database
{
    public class SeedDatabase
    {
        public IList<Instrument> SeedInstrument()
        {
            var instruments = new List<Instrument>();
            var guitar = new Instrument
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Fender",
                Description = "Guitarra acústica",
                Price = 190m,
                CreationDateUtc = DateTime.Now,
                Stock = 1,
                Type = InstrumentType.Stringed
            };

            var flauta = new Instrument
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Yamaña",
                Description = "Flauta Dulce",
                Price = 20m,
                CreationDateUtc = DateTime.Now,
                Stock = 1,
                Type = InstrumentType.Wind
            };

            instruments.Add(flauta);
            instruments.Add(guitar);
            return instruments;
        }
    }
}
