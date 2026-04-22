using AutoMapper;
using Microsoft.Extensions.Logging;
using TiendaMusica.Application.Dtos;
using TiendaMusica.Application.UseCases.Instruments;
using TiendaMusica.Domain.Dtos;
using TiendaMusica.Infrastructure.Entrypoint.Cli.Dtos;
using TiendaMusica.Utilities;

namespace TiendaMusica.Infrastructure.Entrypoint.Cli.Commands
{
    public class InstrumentsCommand
    {
        private readonly IInstrumentUseCase _instrumentUseCase;
        private readonly IMapper _mapper;
        private readonly ILogger<InstrumentsCommand> _logger;
        private readonly ITools _tools;

        public InstrumentsCommand(
            IInstrumentUseCase instrumentUseCase,
            IMapper mapper,
            ILogger<InstrumentsCommand> logger,
            ITools tools
            )
        {
            _instrumentUseCase = instrumentUseCase;
            _mapper = mapper;
            _logger = logger;
            _tools = tools;
        }

        public async Task GetAllAsync(InstrumentGetAllQueryParametersDto? query)
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
                    Console.WriteLine("╔═══════════════════════════════════════════════════════╗");
                    Console.WriteLine("║          Catálogo instrumentos musicales              ║");
                    Console.WriteLine("╚═══════════════════════════════════════════════════════╝");
                    Console.ForegroundColor = ConsoleColor.Green;
                    instrumentsResult.Result.ToList().ForEach(instrument =>
                    {
                        Console.WriteLine($"Id: {instrument.Id}, Name: {instrument.Name}, CreationDate: {_tools.DateTimeUtcToBogotaAsString(instrument.CreationDateUtc)}");
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

        public async Task GetByIdAsync(string id)
        {
            try
            {
                _logger.LogInformation("(Entrypoint CLI) - Iniciando proceso para obtener instrumento por ID: {InstrumentId}", id);

                if (string.IsNullOrWhiteSpace(id))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error: El ID del instrumento no puede estar vacío");
                    Console.ResetColor();
                    _logger.LogWarning("(Entrypoint CLI) - ID vacío proporcionado");
                    return;
                }

                var instrumentResult = await _instrumentUseCase.GetByIdAsync(id);

                if (instrumentResult.HasErrors)
                {
                    Console.ForegroundColor = ConsoleColor.Red;

                    instrumentResult.Errors.ForEach(error =>
                    {
                        Console.WriteLine($"Error Code: {error.ErrorCode}, Message: {error.Message}");
                    });

                    Console.ResetColor();
                    _logger.LogWarning("(Entrypoint CLI) - Se encontraron errores al obtener el instrumento por ID llamando al caso de uso: {Errors}", instrumentResult.Errors);
                }
                else if (instrumentResult.Result == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Instrumento no encontrado con ID: {id}");
                    Console.ResetColor();
                    _logger.LogWarning("(Entrypoint CLI) - Instrumento no encontrado con ID: {InstrumentId}", id);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    var instrument = instrumentResult.Result;
                    Console.WriteLine($"Id: {instrument.Id}, Name: {instrument.Name}, Type: {instrument.Type}, CreationDate: {_tools.DateTimeUtcToBogotaAsString(instrument.CreationDateUtc)}");
                    Console.ResetColor();
                    _logger.LogInformation("(Entrypoint CLI) - Proceso para obtener instrumento por ID finalizado exitosamente con ID: {InstrumentId}", id);
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

                var instrumentCommand = _mapper.Map<InstrumentCreateCommand>(command);
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
                    Console.WriteLine($"Id: {instrumentCreateResult.Result.Id}, Name: {instrumentCreateResult.Result.Name}, Type: {instrumentCreateResult.Result.Type},CreationDate: {_tools.DateTimeUtcToBogotaAsString(instrumentCreateResult.Result.CreationDateUtc)}");
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

        public async Task UpdateAsync(InstrumentUpdateCliRequest command)
        {
            try
            {
                _logger.LogInformation("(Entrypoint CLI) - Iniciando proceso para actualizar instrumento con ID: {InstrumentId}", command.Id);

                var instrumentCommand = _mapper.Map<InstrumentUpdateCommand>(command);
                var instrumentUpdateResult = await _instrumentUseCase.UpdateAsync(instrumentCommand);

                if (instrumentUpdateResult.HasErrors)
                {
                    Console.ForegroundColor = ConsoleColor.Red;

                    instrumentUpdateResult.Errors.ForEach(error =>
                    {
                        Console.WriteLine($"Error Code: {error.ErrorCode}, Message: {error.Message}");
                    });

                    Console.ResetColor();

                    _logger.LogWarning("(Entrypoint CLI) - Se encontraron errores al actualizar el instrumento llamando al caso de uso: {Errors}", instrumentUpdateResult.Errors);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Instrumento actualizado exitosamente:");
                    Console.WriteLine($"Id: {instrumentUpdateResult.Result.Id}");
                    Console.WriteLine($"Name: {instrumentUpdateResult.Result.Name}");
                    Console.WriteLine($"Type: {instrumentUpdateResult.Result.Type}");
                    Console.WriteLine($"Description: {instrumentUpdateResult.Result.Description}");
                    Console.WriteLine($"Price: {instrumentUpdateResult.Result.Price}");
                    Console.WriteLine($"Stock: {instrumentUpdateResult.Result.Stock}");
                    Console.ResetColor();
                    _logger.LogInformation("(Entrypoint CLI) - Proceso para actualizar instrumento finalizado exitosamente con ID: {InstrumentId}", instrumentUpdateResult.Result.Id);
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

        public async Task DeleteMultipleAsync(IList<string> instrumentIds)
        {
            try
            {
                _logger.LogInformation("(Entrypoint CLI) - Iniciando proceso para eliminar múltiples instrumentos: {InstrumentIds}", string.Join(", ", instrumentIds));

                if (instrumentIds == null || instrumentIds.Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error: La lista de IDs no puede estar vacía");
                    Console.ResetColor();
                    _logger.LogWarning("(Entrypoint CLI) - Lista de IDs vacía proporcionada");
                    return;
                }

                var command = new InstrumentDeleteMultipleCommand(instrumentIds);
                var deleteResult = await _instrumentUseCase.DeleteMultipleAsync(command);

                if (deleteResult.HasErrors)
                {
                    Console.ForegroundColor = ConsoleColor.Red;

                    deleteResult.Errors.ForEach(error =>
                    {
                        Console.WriteLine($"Error Code: {error.ErrorCode}, Message: {error.Message}");
                    });

                    Console.ResetColor();
                    _logger.LogWarning("(Entrypoint CLI) - Se encontraron errores al eliminar múltiples instrumentos llamando al caso de uso: {Errors}", deleteResult.Errors);
                }
                else
                {
                    Console.WriteLine("--------------------------------------------");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Eliminación masiva completada exitosamente");
                    Console.WriteLine($"Total de instrumentos eliminados: {deleteResult.Result}");
                    Console.ResetColor();
                    Console.WriteLine("--------------------------------------------");

                    _logger.LogInformation("(Entrypoint CLI) - Proceso para eliminar múltiples instrumentos finalizado exitosamente. {Count} instrumentos eliminados", deleteResult.Result);
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
