using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;
using TiendaMusica.Domain.Enums;
using TiendaMusica.Domain.Models.Result;
using TiendaMusica.Infrastructure.Entrypoint.Rest.Dtos;

namespace TiendaMusica.Infrastructure.Entrypoint.Rest.Utilities.Examples
{
    [ExcludeFromCodeCoverage]
    internal class InstrumentUpdateResponseExample : IMultipleExamplesProvider<Results<InstrumentResponse>>
    {
        private readonly string _dateFormat = "dd-MM-yyyy HH:mm:ss";
        public IEnumerable<SwaggerExample<Results<InstrumentResponse>>> GetExamples()
        {
            var response = new Results<InstrumentResponse>();

            var instrument = new InstrumentResponse(
                Id: "1",
                Name: "Nombre Ejemplo Instrumento Actualizado",
                Description: "Descripción ejemplo instrumento actualizado.",
                Type: InstrumentType.Stringed,
                Price: 1299.99m,
                Stock: 15
            )
            {
                CreationDateUtc = DateTime.UtcNow.ToString(_dateFormat)
            };

            response.Result = instrument;

            yield return SwaggerExample.Create("UpdateInstrumentResponseExample", response);
        }
    }
}
