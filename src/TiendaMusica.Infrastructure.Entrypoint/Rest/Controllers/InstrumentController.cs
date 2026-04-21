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
using TiendaMusica.Domain.Dtos;
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
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(InstrumentGetAllResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InstrumentErrorInternalServerResponseExample))]
        [ProducesResponseType(typeof(Results<IList<InstrumentResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Results<>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Results<>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery, SwaggerParameter("Dirección de ordenamiento, ascendente o descendente: Asc, Desc")] SortDirection sortDirection = SortDirection.Desc,
            [FromQuery, SwaggerParameter("Campo para agregar ordenamiento")] string? orderBy = null,
            [FromQuery, SwaggerParameter("Número de página para paginación, valor entero positivo")] int pageNumber = 1,
            [FromQuery, SwaggerParameter("Tamaño de página para paginación, valor entero positivo")] int pageSize = 10,
            [FromQuery, SwaggerParameter("Término de búsqueda para filtrar instrumentos por nombre y descripción")] string? search = null
            )
        {
            _logger.LogInformation("(endpoint api rest) - Iniciando  proceso para obtener todos los instrumentos");
            var response = new Results<IList<InstrumentResponse>>();
            var query = new InstrumentGetAllQueryParametersDto(search, orderBy, pageNumber, pageSize,sortDirection);
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

        [HttpGet("{id}")]
        [EnableRateLimiting("read")]
        [SwaggerOperation(Summary = "Permite obtener un instrumento por ID", Description = "Permite obtener un instrumento específico del catálogo por su identificador.")]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(InstrumentGetAllResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(InstrumentErrorInternalServerResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InstrumentErrorInternalServerResponseExample))]
        [ProducesResponseType(typeof(Results<InstrumentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Results<>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Results<>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByIdAsync([FromRoute] string id)
        {
            _logger.LogInformation("(endpoint api rest) - Iniciando proceso para obtener instrumento por ID: {InstrumentId}", id);
            var response = new Results<InstrumentResponse>();
            var instrument = await _instrumentUseCase.GetByIdAsync(id);

            if (instrument.HasErrors)
            {
                response.AddErrors(instrument.Errors);
                _logger.LogWarning("(endpoint api rest) - Se encontraron errores al obtener el instrumento por ID llamando al caso de uso: {Errors}", instrument.Errors);
            }
            else
            {
                response.Result = _mapper.Map<InstrumentResponse>(instrument.Result);
                _logger.LogInformation("(endpoint api rest) - Proceso para obtener instrumento por ID finalizado exitosamente con ID: {InstrumentId}", id);
            }

            int statusCode = _restTools.GetHttpStatusCode(response.Errors);
            _logger.LogInformation("(endpoint api rest) - Retornando respuesta con código de estado {StatusCode} para la solicitud de obtener instrumento por ID", statusCode);
            return StatusCode(statusCode, response);
        }

        [HttpPost]
        [EnableRateLimiting("write")]
        [SwaggerOperation(Summary = "Permite crear un instrumento", Description = "Permite crear un instrumento que existen para el catálogo con los tipos Stringed - Wind - keyboard.")]
        [SwaggerResponseExample(StatusCodes.Status201Created, typeof(InstrumentCreateResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(InstrumentErrorBadRequestResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InstrumentErrorInternalServerResponseExample))]
        [ProducesResponseType(typeof(Results<InstrumentResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(Results<>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Results<>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] InstrumentCreateRequest request)
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

            var instrumentCommand = _mapper.Map<InstrumentCreateCommand>(request);

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

        [HttpDelete("delete-multiple")]
        [EnableRateLimiting("write")]
        [SwaggerOperation(Summary = "Permite eliminar múltiples instrumentos", Description = "Permite eliminar varios instrumentos del catálogo proporcionando una lista de IDs.")]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(InstrumentsDeleteMultipleResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(InstrumentErrorBadRequestResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InstrumentErrorInternalServerResponseExample))]
        [ProducesResponseType(typeof(Results<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Results<>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Results<>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteMultipleAsync([FromBody] InstrumentDeleteMultipleRequest request)
        {
            _logger.LogInformation("(endpoint api rest) - Iniciando proceso para eliminar múltiples instrumentos: {InstrumentIds}", string.Join(", ", request.InstrumentIds));
            var response = new Results<int>();

            var command = _mapper.Map<InstrumentDeleteMultipleCommand>(request);
            var deleteResult = await _instrumentUseCase.DeleteMultipleAsync(command);

            if (deleteResult.HasErrors)
            {
                response.AddErrors(deleteResult.Errors);
                _logger.LogWarning("(endpoint api rest) - Se encontraron errores al eliminar múltiples instrumentos llamando al caso de uso: {Errors}", deleteResult.Errors);
            }
            else
            {
                response.Result = deleteResult.Result;
                _logger.LogInformation("(endpoint api rest) - Proceso para eliminar múltiples instrumentos finalizado exitosamente. {Count} instrumentos eliminados", response.Result);
            }

            int statusCode = _restTools.GetHttpStatusCode(response.Errors);
            _logger.LogInformation("(endpoint api rest) - Retornando respuesta con código de estado {StatusCode} para la solicitud de eliminación de múltiples instrumentos", statusCode);
            return StatusCode(statusCode, response);
        }

        [HttpPut("{id}")]
        [EnableRateLimiting("write")]
        [SwaggerOperation(Summary = "Permite actualizar un instrumento", Description = "Permite actualizar un instrumento existente en el catálogo.")]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(InstrumentUpdateResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(InstrumentErrorBadRequestResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(InstrumentErrorInternalServerResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InstrumentErrorInternalServerResponseExample))]
        [ProducesResponseType(typeof(Results<InstrumentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Results<>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Results<>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Results<>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAsync([FromRoute] string id, [FromBody] InstrumentUpdateRequest request)
        {
            _logger.LogInformation("(endpoint api rest) - Iniciando proceso para actualizar instrumento con ID: {InstrumentId}", id);
            var response = new Results<InstrumentResponse>();

            if (id != request.Id)
            {
                response.AddError(ErrorCode.VALIDATION_ERROR, "El ID en la ruta no coincide con el ID en el cuerpo de la solicitud");
                _logger.LogWarning("(endpoint api rest) - El ID en la ruta no coincide con el ID en el cuerpo de la solicitud");
                return BadRequest(response);
            }

            var validator = new InstrumentUpdateRequestValidator();
            var result = validator.Validate(request);

            if (!result.IsValid)
            {
                response.AddErrors(result.Errors.Select(error => new TiendaMusicaError(
                   ErrorCode.VALIDATION_ERROR,
                   $"Error en la propiedad {error.PropertyName}: {error.ErrorMessage}")
                ));
                _logger.LogWarning("(endpoint api rest) - Validación fallida del request para la solicitud de actualización de instrumento: {Errors}", response.Errors);
                return BadRequest(response);
            }

            var instrumentCommand = _mapper.Map<InstrumentUpdateCommand>(request);

            var instrumentUpdate = await _instrumentUseCase.UpdateAsync(instrumentCommand);

            if (instrumentUpdate.HasErrors)
            {
                response.AddErrors(instrumentUpdate.Errors);
                _logger.LogWarning("(endpoint api rest) - Se encontraron errores al actualizar el instrumento llamando al caso de uso: {Errors}", instrumentUpdate.Errors);
            }
            else
            {
                response.Result = _mapper.Map<InstrumentResponse>(instrumentUpdate.Result);
                _logger.LogInformation("(endpoint api rest) - Proceso para actualizar instrumento finalizado exitosamente con ID: {InstrumentId}", response.Result.Id);
            }

            int statusCode = _restTools.GetHttpStatusCode(response.Errors);
            _logger.LogInformation("(endpoint api rest) - Retornando respuesta con código de estado {StatusCode} para la solicitud de actualización de instrumento", statusCode);
            return StatusCode(statusCode, response);
        }
    }
}
