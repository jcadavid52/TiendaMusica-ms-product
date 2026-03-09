using Swashbuckle.AspNetCore.Filters;
using TiendaMusica.Domain.Models.Result;
using TiendaMusica.Infrastructure.Entrypoint.Rest.Dtos;

namespace TiendaMusica.Infrastructure.Entrypoint.Rest.Utilities.Examples
{
    internal class ErrorBadRequestInstrumentResponseExample : IMultipleExamplesProvider<Results<InstrumentResponse>>
    {
        public IEnumerable<SwaggerExample<Results<InstrumentResponse>>> GetExamples()
        {
            var response = new Results<InstrumentResponse>();

            response.AddError(ErrorCode.VALIDATION_ERROR, "Error en el request de la solicitud, revise e intente nuevamente");

            yield return SwaggerExample.Create("ErrorInternalServerInstrumentResponseExample", response);
        }
    }
}
