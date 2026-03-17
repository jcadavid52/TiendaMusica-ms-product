using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;
using TiendaMusica.Domain.Enums;
using TiendaMusica.Domain.Models.Result;
using TiendaMusica.Infrastructure.Entrypoint.Rest.Dtos;

namespace TiendaMusica.Infrastructure.Entrypoint.Rest.Utilities.Examples
{
    [ExcludeFromCodeCoverage]
    internal class CreateInstrumentResponseExample : IMultipleExamplesProvider<Results<InstrumentResponse>>
    {
        public IEnumerable<SwaggerExample<Results<InstrumentResponse>>> GetExamples()
        {
            var response = new Results<InstrumentResponse>();

            var instrument = new InstrumentResponse(
                    "1",
                    "Guitar",
                    "A string instrument",
                    InstrumentType.Stringed,
                    199.99m,
                    10);

            response.Result = instrument;

            yield return SwaggerExample.Create("CreateInstrumentResponseExample", response);
        }
    }
}
