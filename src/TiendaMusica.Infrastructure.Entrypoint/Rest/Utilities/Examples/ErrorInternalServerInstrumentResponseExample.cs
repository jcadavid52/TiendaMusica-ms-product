using Swashbuckle.AspNetCore.Filters;
using TiendaMusica.Domain.Models.Result;
using TiendaMusica.Infrastructure.Entrypoint.Rest.Dtos;

namespace TiendaMusica.Infrastructure.Entrypoint.Rest.Utilities.Examples
{
    internal class ErrorInternalServerInstrumentResponseExample : IMultipleExamplesProvider<Results<InstrumentResponse>>
    {
        public IEnumerable<SwaggerExample<Results<InstrumentResponse>>> GetExamples()
        {
            var response = new Results<InstrumentResponse>();

            response.AddError(ErrorCode.SERVER_ERROR, "An unexpected error occurred while processing the request.");

            yield return SwaggerExample.Create("ErrorInternalServerInstrumentResponseExample", response);
        }
    }
}
