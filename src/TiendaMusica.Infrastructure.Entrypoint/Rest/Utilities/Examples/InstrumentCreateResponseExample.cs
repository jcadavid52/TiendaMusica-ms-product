using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;
using TiendaMusica.Domain.Enums;
using TiendaMusica.Domain.Models.Result;
using TiendaMusica.Infrastructure.Entrypoint.Rest.Dtos;

namespace TiendaMusica.Infrastructure.Entrypoint.Rest.Utilities.Examples
{
    [ExcludeFromCodeCoverage]
    internal class InstrumentCreateResponseExample : IMultipleExamplesProvider<Results<InstrumentResponse>>
    {
        private readonly string _dateFormat = "dd-MM-yyyy HH:mm:ss";
        public IEnumerable<SwaggerExample<Results<InstrumentResponse>>> GetExamples()
        {
            var response = new Results<InstrumentResponse>();

            var instrument = new InstrumentResponse(
                Id: "1",
                Name: "Nombre Ejemplo Instrumento Creado",
                Description: "Descripción ejemplo instrumento creado.",
                Type: InstrumentType.Stringed,
                Price: 999.99m,
                Stock: 10
            )
            {
                CreationDateUtc = DateTime.UtcNow.ToString(_dateFormat)
            };

            response.Result = instrument;

            yield return SwaggerExample.Create("CreateInstrumentResponseExample", response);
        }
    }
}
