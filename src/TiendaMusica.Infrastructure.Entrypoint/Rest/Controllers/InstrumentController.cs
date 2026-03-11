using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System.Net;
using TiendaMusica.Application.UseCases;
using TiendaMusica.Domain.Models;
using TiendaMusica.Domain.Models.Result;
using TiendaMusica.Infrastructure.Entrypoint.Rest.Dtos;
using TiendaMusica.Infrastructure.Entrypoint.Rest.Utilities;
using TiendaMusica.Infrastructure.Entrypoint.Rest.Utilities.Examples;
using TiendaMusica.Infrastructure.Entrypoint.Rest.Validators;
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
        [SwaggerOperation(Summary = "Permite listar instrumentos", Description = "Permite obtener todos los instrumentos que existen en el catálogo.")]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(GetInstrumentsResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(ErrorInternalServerInstrumentResponseExample))]
        [ProducesResponseType(typeof(Results<IList<InstrumentResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Results<>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Results<>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllAsync()
        {
            var response = new Results<IList<InstrumentResponse>>();

            try
            {
                var instruments = await _instrumentUseCase.GetAllAsync();

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
        [SwaggerOperation(Summary = "Permite crear un instrumento", Description = "Permite crear un instrumento que existen para el catálogo.")]
        [SwaggerResponseExample(StatusCodes.Status201Created, typeof(CreateInstrumentResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(ErrorBadRequestInstrumentResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(ErrorInternalServerInstrumentResponseExample))]
        [ProducesResponseType(typeof(Results<InstrumentResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(Results<>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Results<>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] InstrumentRequest request)
        {
            var response = new Results<InstrumentResponse>();
            try
            {
                var validator = new InstrumentRequestValidator();
                var result = validator.Validate(request);

                if (!result.IsValid)
                {
                    response.AddErrors(result.Errors.Select(error => new TiendaMusicaError(
                       ErrorCode.VALIDATION_ERROR,
                       $"Error en la propiedad {error.PropertyName}: {error.ErrorMessage}")
                    ));
                    return BadRequest(response);
                }

                var instrument = _mapper.Map<Instrument>(request);

                var instrumentCreate = await _instrumentUseCase.CreateAsync(instrument);

                if (instrumentCreate.HasErrors)
                {
                    response.AddErrors(instrumentCreate.Errors);
                }

                response.Result = _mapper.Map<InstrumentResponse>(instrumentCreate.Result);
            }
            catch (ArgumentException ex)
            {
                var error = ex.ToString();
                response.AddError(ErrorCode.VALIDATION_ERROR, $"Error creando instrumento-Endpoint-Create {error}");
            }
            catch (Exception ex)
            {
                var error = ex.ToString();
                response.AddError(ErrorCode.SERVER_ERROR, $"Error creando instrumento-Endpoint-Create {error}");
            }
            int statusCode = _restTools.GetHttpStatusCode(response.Errors, (int)HttpStatusCode.Created);
            return StatusCode(statusCode, response);
        }
    }
}
