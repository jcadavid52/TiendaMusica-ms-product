using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;
using TiendaMusica.Domain.Dtos;
using TiendaMusica.Domain.Enums;
using TiendaMusica.Domain.Models.Result;
using TiendaMusica.Infrastructure.Entrypoint.Rest.Dtos;

namespace TiendaMusica.Infrastructure.Entrypoint.Rest.Utilities.Examples
{
    [ExcludeFromCodeCoverage]
    internal class InstrumentGetAllResponseExample : IMultipleExamplesProvider<Results<IList<InstrumentResponse>>>
    {
        private readonly string _dateFormat = "dd-MM-yyyy HH:mm:ss";
        public IEnumerable<SwaggerExample<Results<IList<InstrumentResponse>>>> GetExamples()
        {
            var response = new Results<IList<InstrumentResponse>>();

            var instruments = new List<InstrumentResponse>
            {
                new InstrumentResponse(
                    Id: "1",
                    Name: "Ejemplo nombre instrumento 1",
                    Description: "Ejemplo descripción instrumento 1",
                    Type: InstrumentType.Stringed,
                    Price: 999.99m,
                    Stock: 10,
                    Category: new CategoryDto(Id: 1, Name: "Instrument", Description: "Descripción ejemplo categoría.")
                    )
                {
                    CreationDateUtc = DateTime.UtcNow.AddDays(-30).ToString(_dateFormat)
                },
                new InstrumentResponse(
                    Id: "2",
                   Name: "Ejemplo nombre instrumento 2",
                    Description: "Ejemplo descripción instrumento 2",
                    Type: InstrumentType.Stringed,
                    Price: 999.99m,
                    Stock: 15,
                    Category: new CategoryDto(Id: 1, Name: "Instrument", Description: "Descripción ejemplo categoría.")
                    )
                {
                    CreationDateUtc = DateTime.UtcNow.AddDays(-15).ToString(_dateFormat)
                }
            };

            response.Result = instruments;

            yield return SwaggerExample.Create("GetInstrumentsResponseExample", response);
        }
    }
}
