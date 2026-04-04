using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;
using TiendaMusica.Domain.Models.Result;

namespace TiendaMusica.Infrastructure.Entrypoint.Rest.Utilities.Examples
{
    [ExcludeFromCodeCoverage]
    internal class DeleteMultipleInstrumentsResponseExample : IMultipleExamplesProvider<Results<int>>
    {
        public IEnumerable<SwaggerExample<Results<int>>> GetExamples()
        {
            var response = new Results<int>
            {
                Result = 3
            };

            yield return SwaggerExample.Create("DeleteMultipleInstrumentsResponseExample - Éxito", response);

            var errorResponse = new Results<int>();
            errorResponse.AddError(ErrorCode.VALIDATION_ERROR, "La lista de IDs no puede estar vacía");

            yield return SwaggerExample.Create("DeleteMultipleInstrumentsResponseExample - Error de Validación", errorResponse);
        }
    }
}
