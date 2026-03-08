using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using TiendaMusica.Application.UseCases;
using TiendaMusica.Domain.Models;
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

        [HttpPost]
        public IActionResult Create([FromBody]InstrumentRequest request)
        {
            var response = new Results<InstrumentResponse>();
            try
            {
                var instrument = _mapper.Map<Instrument>(request);

                var instrumentCreate = _instrumentUseCase.Create(instrument);

                if (instrumentCreate.HasErrors)
                {
                    response.AddErrors(instrumentCreate.Errors);
                }

                response.Result = _mapper.Map<InstrumentResponse>(instrumentCreate.Result);
            }
            catch(ArgumentException ex)
            {
                var error = ex.ToString();
                response.AddError(ErrorCode.VALIDATION_ERROR,$"Error creando instrumento-Endpoint-Create {error}");
            }
            catch (Exception ex)
            {
                var error = ex.ToString();
                response.AddError(ErrorCode.SERVER_ERROR, $"Error creando instrumento-Endpoint-Create {error}");
            }
            int statusCode = _restTools.GetHttpStatusCode(response.Errors,(int)HttpStatusCode.Created);
            return StatusCode(statusCode, response);
        }
    }
}
