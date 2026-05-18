using System.Net;
using TiendaMusica.Domain.Models.Result;

namespace TiendaMusica.Infrastructure.Entrypoint.Rest.Utilities
{
    public interface IRestTools
    {
        int GetHttpStatusCode(List<TiendaMusicaError>? errors, int successStatusCode = (int)HttpStatusCode.OK);
    }
}
