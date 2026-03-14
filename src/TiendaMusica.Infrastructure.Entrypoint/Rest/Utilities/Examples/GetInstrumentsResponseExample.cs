using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;
using TiendaMusica.Domain.Models;
using TiendaMusica.Domain.Models.Result;
using TiendaMusica.Infrastructure.Entrypoint.Rest.Dtos;

namespace TiendaMusica.Infrastructure.Entrypoint.Rest.Utilities.Examples
{
    [ExcludeFromCodeCoverage]
    internal class GetInstrumentsResponseExample : IMultipleExamplesProvider<Results<IList<InstrumentResponse>>>
    {
        public IEnumerable<SwaggerExample<Results<IList<InstrumentResponse>>>> GetExamples()
        {
            var response = new Results<IList<InstrumentResponse>>();

            var instruments = new List<InstrumentResponse>
            {
                new InstrumentResponse(
                    "1",
                    "Guitar",
                    "A string instrument",
                    InstrumentType.Stringed,
                    199.99m,
                    10),
                new InstrumentResponse(
                    "2",
                    "Piano",
                    "A keyboard instrument",
                    InstrumentType.Wind,
                    499.99m,
                    5)
            };

            response.Result = instruments;

            yield return SwaggerExample.Create("GetInstrumentsResponseExample", response);
        }
    }
}
