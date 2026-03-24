using AutoMapper;
using Microsoft.Extensions.Logging;
using TiendaMusica.Application.Dtos;
using TiendaMusica.Application.UseCases.Instruments;
using TiendaMusica.Infrastructure.Entrypoint.Cli.Dtos;

namespace TiendaMusica.Infrastructure.Entrypoint.Cli.Commands
{
    public class InstrumentsCommand
    {
        private readonly IInstrumentUseCase _instrumentUseCase;
        private readonly IMapper _mapper;
        private readonly ILogger<InstrumentsCommand> _logger;

        public InstrumentsCommand(
            IInstrumentUseCase instrumentUseCase,
            IMapper mapper,
            ILogger<InstrumentsCommand> logger
            )
        {
            _instrumentUseCase = instrumentUseCase;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task GetAllAsync(GetAllInstrumentQuery? query = null)
        {
            try
            {
                _logger.LogInformation("(Entrypoint CLI) - Iniciando  proceso para obtener todos los instrumentos");

                var instrumentsResult = await _instrumentUseCase.GetAllAsync(query);

                if (instrumentsResult.HasErrors)
                {
                    Console.ForegroundColor = ConsoleColor.Red;

                    instrumentsResult.Errors.ForEach(error =>
                    {
                        Console.WriteLine($"Error Code: {error.ErrorCode}, Message: {error.Message}");
                    });

                    Console.ResetColor();
                    _logger.LogWarning("(Entrypoint CLI) - Se encontraron errores al obtener los instrumentos llamando al caso de uso: {Errors}", instrumentsResult.Errors);
                }
                else
                {
                    Console.WriteLine("--------------------------------------------");
                    Console.WriteLine("Catálogo instrumentos musicales");
                    Console.WriteLine("--------------------------------------------");
                    Console.ForegroundColor = ConsoleColor.Green;
                    instrumentsResult.Result.ToList().ForEach(instrument =>
                    {
                        Console.WriteLine($"Id: {instrument.Id}, Name: {instrument.Name}, Type: {instrument.Type}");
                    });
                    Console.ResetColor();
                    Console.WriteLine("--------------------------------------------");

                    _logger.LogInformation("(Entrypoint CLI) - Proceso para obtener todos los instrumentos finalizado exitosamente con {Count} instrumentos", instrumentsResult.Result.Count);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ocurrió una excepción no controlada: {ex.Message}");
                Console.ResetColor();
                _logger.LogError(ex, "Excepción no controlada: {Message}", ex.Message);
            }
        }

        public async Task CreateAsync(InstrumentCreateCliRequest command)
        {
            try
            {
                _logger.LogInformation("(Entrypoint CLI) - Iniciando proceso para crear un nuevo instrumento con los datos: {@Command}", command);

                var instrumentCommand = _mapper.Map<CreateInstrumentCommand>(command);
                var instrumentCreateResult = await _instrumentUseCase.CreateAsync(instrumentCommand);

                if (instrumentCreateResult.HasErrors)
                {
                    Console.ForegroundColor = ConsoleColor.Red;

                    instrumentCreateResult.Errors.ForEach(error =>
                    {
                        Console.WriteLine($"Error Code: {error.ErrorCode}, Message: {error.Message}");
                    });

                    Console.ResetColor();

                    _logger.LogWarning("(Entrypoint CLI) - Se encontraron errores al crear el instrumento llamando al caso de uso: {Errors}", instrumentCreateResult.Errors);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Id: {instrumentCreateResult.Result.Id}, Name: {instrumentCreateResult.Result.Name}, Type: {instrumentCreateResult.Result.Type}");
                    Console.ResetColor();
                    _logger.LogInformation("(Entrypoint CLI) - Proceso para crear un nuevo instrumento finalizado exitosamente con el ID: {InstrumentId}", instrumentCreateResult.Result.Id);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ocurrió una excepción no controlada: {ex.Message}");
                Console.ResetColor();
                _logger.LogError(ex, "Excepción no controlada: {Message}", ex.Message);
            }
        }
    }
}
