using Microsoft.AspNetCore.Http;
using TiendaMusica.Domain.Models.Result;

namespace TiendaMusica.Infrastructure.Entrypoint.Rest.Utilities
{
    internal class RestTools : IRestTools
    {
        public int GetHttpStatusCode(List<TiendaMusicaError> errors)
        {
            if (errors == null || errors.Count == 0)
            {
                return StatusCodes.Status200OK;
            }

            return errors.Select(e =>
                    e.ErrorCode == ErrorCode.CONFLICT_ERROR ? StatusCodes.Status409Conflict
                    : StatusCodes.Status500InternalServerError).First();
        }
    }
}
