using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TiendaMusica.Application.UseCases;
using TiendaMusica.Domain.Models.Result;
using TiendaMusica.Infrastructure.Entrypoint.Rest.Dtos;
using TiendaMusica.Infrastructure.Entrypoint.Rest.Utilities;
namespace TiendaMusica.Infrastructure.Entrypoint.Rest.Controllers
{
    [ApiController]
    [Route("v1/instrument")]
    [Produces("application/json")]
    public class InstrumentController : ControllerBase
    {
        private readonly IInstrumentUseCase _instrumentUseCase;
        private readonly IMapper _mapper;
        private readonly IRestTools _restTools;

        public InstrumentController(
            IInstrumentUseCase instrumentUseCase,
            IMapper mapper,
            IRestTools restTools
            )
        {
            _instrumentUseCase = instrumentUseCase;
            _mapper = mapper;
            _restTools = restTools;
        }
        [HttpGet]
        public IActionResult GetAll()
        {
            var response = new Results<IList<InstrumentResponse>>();

            try
            {
                var instruments = _instrumentUseCase.GetAll();

                if (instruments.HasErrors)
                {
                    response.AddErrors(instruments.Errors);
                }

                response.Result = instruments.Result.Select(x => _mapper.Map<InstrumentResponse>(x)).ToList();
            }
            catch (Exception ex)
            {
                var error = ex.ToString();
                response.AddError(ErrorCode.SERVER_ERROR, $"Error obteniendo instrumentos-Endpoint-GetAll {error}");
            }

            int statusCode = _restTools.GetHttpStatusCode(response.Errors);
            return StatusCode(statusCode, response);
        }
    }
}
