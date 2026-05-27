using AutoMapper;
using TiendaMusica.Domain.Models;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb.Documents;

namespace TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb.Mappers
{
    public class InstrumentDocumentToInstrumentConverter : ITypeConverter<InstrumentDocument, Instrument>
    {
        public Instrument Convert(InstrumentDocument source, Instrument destination, ResolutionContext context)
        {
            var result = Instrument.Create(
                source.Name,
                source.Description,
                source.Type,
                source.Price,
                source.Stock,
                source.CategoryId
            );

            var instrument = result.Result!;

            instrument.Id = source.Id;
            instrument.CreationDateUtc = source.CreationDateUtc;

            if (source.Category != null)
            {
                var category = new Category(
                    source.Category.Id,
                    source.Category.Name,
                    source.Category.Description
                );
                typeof(Product).GetProperty(nameof(Product.Category))?.SetValue(instrument, category);
            }

            return instrument;
        }
    }
}
