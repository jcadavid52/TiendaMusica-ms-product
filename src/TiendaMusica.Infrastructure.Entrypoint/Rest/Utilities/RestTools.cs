using System.Net;
using TiendaMusica.Domain.Models.Result;

namespace TiendaMusica.Infrastructure.Entrypoint.Rest.Utilities
{
    internal class RestTools : IRestTools
    {
        public int GetHttpStatusCode(List<TiendaMusicaError>? errors, int successStatusCode = (int)HttpStatusCode.OK)
        {
            if (errors == null || errors.Count == 0)
            {
                return successStatusCode;
            }

            return errors.Select(e => e.ErrorCode == ErrorCode.CONFLICT_ERROR ? (int)HttpStatusCode.Conflict
                : e.ErrorCode == ErrorCode.VALIDATION_ERROR ? (int)HttpStatusCode.BadRequest
                : e.ErrorCode == ErrorCode.LIMIT_STOCK_ERROR ? (int)HttpStatusCode.BadRequest
                : e.ErrorCode == ErrorCode.MINIMUM_STOCK_ERROR ? (int)HttpStatusCode.BadRequest
                : e.ErrorCode == ErrorCode.NOT_FOUND ? (int)HttpStatusCode.NotFound
                : e.ErrorCode == ErrorCode.PUBLISH_MESSAGE_ERROR ? (int)HttpStatusCode.InternalServerError
                : e.ErrorCode == ErrorCode.DATABASE_ERROR ? (int)HttpStatusCode.InternalServerError
                : (int)HttpStatusCode.InternalServerError).First();
        }
    }
}
