using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System.Net;
using TiendaMusica.Application.Dtos;
using TiendaMusica.Application.UseCases.Instruments;
using TiendaMusica.Domain.Enums;
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
        private readonly ILogger<InstrumentController> _logger;

        public InstrumentController(
            IInstrumentUseCase instrumentUseCase,
            IMapper mapper,
            IRestTools restTools,
            ILogger<InstrumentController> logger
            )
        {
            _instrumentUseCase = instrumentUseCase;
            _mapper = mapper;
            _restTools = restTools;
            _logger = logger;
        }
        [HttpGet]
        [EnableRateLimiting("read")]
        [SwaggerOperation(Summary = "Permite listar instrumentos", Description = "Permite obtener todos los instrumentos que existen en el catálogo.")]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(GetInstrumentsResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(ErrorInternalServerInstrumentResponseExample))]
        [ProducesResponseType(typeof(Results<IList<InstrumentResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Results<>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Results<>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery, SwaggerParameter("Dirección de ordenamiento, ascendente o descendente por fecha de creación: Asc, Desc")] SortDirection sortDirection = SortDirection.Desc,
            [FromQuery, SwaggerParameter("Número de página para paginación, valor entero positivo")] int pageNumber = 1,
            [FromQuery, SwaggerParameter("Tamaño de página para paginación, valor entero positivo")] int pageSize = 10,
            [FromQuery, SwaggerParameter("Término de búsqueda para filtrar instrumentos por nombre, descripción o tipo de instrumento")] string? search = null
            )
        {
            _logger.LogInformation("(endpoint api rest) - Iniciando  proceso para obtener todos los instrumentos");
            var response = new Results<IList<InstrumentResponse>>();
            var query = new GetAllInstrumentQuery(sortDirection, search, pageSize, pageNumber);
            var instruments = await _instrumentUseCase.GetAllAsync(query);

            if (instruments.HasErrors)
            {
                response.AddErrors(instruments.Errors);
                _logger.LogWarning("(endpoint api rest) - Se encontraron errores al obtener los instrumentos llamando al caso de uso: {Errors}", instruments.Errors);
            }
            else
            {
                response.Result = _mapper.Map<List<InstrumentResponse>>(instruments.Result);
                _logger.LogInformation("(endpoint api rest) - Proceso para obtener todos los instrumentos finalizado exitosamente con {Count} instrumentos", response.Result.Count);
            }

            int statusCode = _restTools.GetHttpStatusCode(response.Errors);
            _logger.LogInformation("(endpoint api rest) - Retornando respuesta con código de estado {StatusCode} para la solicitud de obtener todos los instrumentos", statusCode);
            return StatusCode(statusCode, response);
        }

        [HttpPost]
        [EnableRateLimiting("write")]
        [SwaggerOperation(Summary = "Permite crear un instrumento", Description = "Permite crear un instrumento que existen para el catálogo con los tipos Stringed - Wind - keyboard.")]
        [SwaggerResponseExample(StatusCodes.Status201Created, typeof(CreateInstrumentResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(ErrorBadRequestInstrumentResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(ErrorInternalServerInstrumentResponseExample))]
        [ProducesResponseType(typeof(Results<InstrumentResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(Results<>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Results<>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] InstrumentRequest request)
        {
            _logger.LogInformation("(endpoint api rest) - Iniciando proceso para crear un nuevo instrumento con los datos: {@Request}", request);
            var response = new Results<InstrumentResponse>();

            var validator = new InstrumentRequestValidator();
            var result = validator.Validate(request);

            if (!result.IsValid)
            {
                response.AddErrors(result.Errors.Select(error => new TiendaMusicaError(
                   ErrorCode.VALIDATION_ERROR,
                   $"Error en la propiedad {error.PropertyName}: {error.ErrorMessage}")
                ));
                _logger.LogWarning("(endpoint api rest) - Validación fallida del resquest para la solicitud de creación de instrumento: {Errors}", response.Errors);
                return BadRequest(response);
            }

            var instrumentCommand = _mapper.Map<CreateInstrumentCommand>(request);

            var instrumentCreate = await _instrumentUseCase.CreateAsync(instrumentCommand);

            if (instrumentCreate.HasErrors)
            {
                response.AddErrors(instrumentCreate.Errors);
                _logger.LogWarning("(endpoint api rest) - Se encontraron errores al crear el instrumento llamando al caso de uso: {Errors}", instrumentCreate.Errors);
            }
            else
            {
                response.Result = _mapper.Map<InstrumentResponse>(instrumentCreate.Result);
                _logger.LogInformation("(endpoint api rest) - Proceso para crear un nuevo instrumento finalizado exitosamente con el ID: {InstrumentId}", response.Result.Id);
            }

            int statusCode = _restTools.GetHttpStatusCode(response.Errors, (int)HttpStatusCode.Created);
            _logger.LogInformation("(endpoint api rest) - Retornando respuesta con código de estado {StatusCode} para la solicitud de creación de instrumento", statusCode);
            return StatusCode(statusCode, response);
        }
    }
}
