using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using TiendaMusica.Application.Dtos;
using TiendaMusica.Domain.Enums;
using TiendaMusica.Infrastructure.Entrypoint.Cli.Commands;
using TiendaMusica.Infrastructure.Entrypoint.Cli.Dtos;
using TiendaMusica.Infrastructure.Entrypoint.Cli.Parameters;

namespace TiendaMusica.Infrastructure.Entrypoint.Cli
{
    [ExcludeFromCodeCoverage]
    public static class MainExecute
    {
        public static async Task ExecuteAsync(string[] args, IServiceProvider serviceProvider)
        {
            try
            {
                Log.Information("Ejecutando comando CLI: {Command}", string.Join(' ', args));
                using var scope = serviceProvider.CreateScope();
                var provider = scope.ServiceProvider;

                var command = args[0].ToLower();
                var instrumentsCommand = provider.GetRequiredService<InstrumentsCommand>();

                switch (command)
                {
                    case InstrumentParameters.List:

                        var query = BuildGetAllQuery();
                        await instrumentsCommand.GetAllAsync(query);
                        Log.Information("Listando todos los instrumentos...");

                        break;
                    case InstrumentParameters.Add:

                        var createCommand = BuildCliCreateRequest(args);
                        await instrumentsCommand.CreateAsync(createCommand);
                        Log.Information("Agregando nuevo instrumento: {Name}", createCommand.Name);

                        break;
                    case InstrumentParameters.Delete:

                        var idsToDelete = BuildDeleteMultipleRequest();
                        if (idsToDelete.Any())
                        {
                            await instrumentsCommand.DeleteMultipleAsync(idsToDelete);
                            Log.Information("Iniciando eliminación masiva de {Count} instrumentos", idsToDelete.Count);
                        }
                        else
                        {
                            Log.Warning("No se proporcionaron IDs para eliminar");
                            Console.WriteLine("No se proporcionaron IDs para eliminar.");
                        }

                        break;
                    case InstrumentParameters.Update:

                        var updateCommand = BuildCliUpdateRequest(args);
                        await instrumentsCommand.UpdateAsync(updateCommand);
                        Log.Information("Actualizando instrumento con ID: {InstrumentId}", updateCommand.Id);

                        break;
                    case InstrumentParameters.GetById:

                        var instrumentId = BuildGetByIdRequest();
                        if (!string.IsNullOrWhiteSpace(instrumentId))
                        {
                            await instrumentsCommand.GetByIdAsync(instrumentId);
                            Log.Information("Buscando instrumento con ID: {InstrumentId}", instrumentId);
                        }
                        else
                        {
                            Log.Warning("No se proporcionó ID para buscar");
                            Console.WriteLine("No se proporcionó ID válido para buscar.");
                        }

                        break;
                    case InstrumentParameters.Help:

                        ShowHelp();

                        break;
                    default:

                        Log.Warning("Intento de agregar comando desconocido");
                        Console.WriteLine($"Comando desconocido: {command}");

                        break;
                }
            }
            catch (ArgumentNullException ex)
            {
                Log.Error(ex, "Argumentos inválidos: {Message}", ex.Message);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Argumentos inválidos: {ex.Message}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Excepción no controlada: {Message}", ex.Message);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Excepción no controlada: {ex.Message}");
                Console.ResetColor();
            }
        }

        private static InstrumentCreateCliRequest BuildCliCreateRequest(string[] args)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("--- Registro de Nuevo Instrumento ---");
            Console.ResetColor();

            Console.Write("Nombre: ");
            string? name = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("El nombre es requerido.");
            }

            Console.Write("Descripción: ");
            string? description = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentNullException("La descripción es requerida.");
            }

            Console.Write("Tipo (Wind | Stringed | Keyboard | Percussion)");
            string? type = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(type))
            {
                throw new ArgumentNullException("La descripción es requerida.");
            }

            if (!Enum.TryParse<InstrumentType>(type, true, out var instrumentType))
            {
                Log.Warning("Intento de agregar instrumento con tipo inválido: {Type}", type);
                throw new ArgumentNullException($"⚠️ '{type}' no es un tipo de instrumento válido.");
            }

            Console.Write("Precio: ");
            decimal price = decimal.Parse(Console.ReadLine() ?? "0");

            if (price <= 0)
            {
                throw new ArgumentNullException("Precio no válido");
            }

            Console.Write("Stock inicial: ");
            int stock = int.Parse(Console.ReadLine() ?? "0");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✅ Creación en proceso...");
            Console.ResetColor();

            return new InstrumentCreateCliRequest(Name: name,
                            Description: description,
                            Type: instrumentType,
                            Price: price,
                            Stock: stock);
        }

        private static void ShowHelp()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("Uso:");
            Console.WriteLine("  instrument-list                - Lista todos los instrumentos");
            Console.WriteLine("  instrument-add                 - Agrega un nuevo instrumento");
            Console.WriteLine("  instrument-update              - Actualiza un instrumento existente");
            Console.WriteLine("  instrument-getbyid             - Obtiene un instrumento por su ID");
            Console.WriteLine("  instrument-delete-multiple     - Elimina múltiples instrumentos por ID");
            Console.WriteLine("  help                           - Muestra esta ayuda");
            Console.WriteLine("\nEjemplos:");
            Console.WriteLine("  instrument-add");
            Console.WriteLine("  instrument-update");
            Console.WriteLine("  instrument-getbyid");
            Console.WriteLine("  instrument-delete-multiple");
            Console.ResetColor();
        }

        private static InstrumentUpdateCliRequest BuildCliUpdateRequest(string[] args)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("╔═══════════════════════════════════════════════════════╗");
            Console.WriteLine("║           Actualización de Instrumento               ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════╝");
            Console.ResetColor();

            Console.Write("ID del instrumento a actualizar: ");
            string? id = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("El ID es requerido.");
            }

            Console.Write("Nombre: ");
            string? name = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("El nombre es requerido.");
            }

            Console.Write("Descripción: ");
            string? description = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentNullException("La descripción es requerida.");
            }

            Console.Write("Tipo (Wind | Stringed | Keyboard | Percussion): ");
            string? type = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(type))
            {
                throw new ArgumentNullException("El tipo es requerido.");
            }

            if (!Enum.TryParse<InstrumentType>(type, true, out var instrumentType))
            {
                Log.Warning("Intento de actualizar instrumento con tipo inválido: {Type}", type);
                throw new ArgumentNullException($"'{type}' no es un tipo de instrumento válido.");
            }

            return new InstrumentUpdateCliRequest(
                Id: id,
                Name: name,
                Description: description,
                Type: instrumentType
            );
        }

        private static InstrumentGetAllQuery? BuildGetAllQuery()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("--- Agregar filtro 1.Si 2.No ---");
            string? choice = Console.ReadLine();
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Magenta;
            if (choice == "1")
            {
                Console.Write("Ingrese término de búsqueda: ");
                string? search = Console.ReadLine();

                Console.WriteLine("Tamaño de página");
                string? pageSizeInput = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(pageSizeInput))
                {
                    pageSizeInput = "10";
                }

                Console.WriteLine("Número de página");
                string? pageNumberInput = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(pageNumberInput))
                {
                    pageNumberInput = "1";
                }

                Console.ResetColor();

                return new InstrumentGetAllQuery(
                    PageSize: int.Parse(pageSizeInput),
                    Search: search,
                    PageNumber: int.Parse(pageNumberInput)
                    );
            }

            Console.ResetColor();

            return null;
        }

        private static IList<string> BuildDeleteMultipleRequest()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("╔═══════════════════════════════════════════════════════╗");
            Console.WriteLine("║          Eliminación Masiva de Instrumentos           ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════╝");
            Console.ResetColor();

            var idsToDelete = new List<string>();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n⚠️  OPERACIÓN DE ELIMINACIÓN - Por favor confirme que desea continuar\n");
            Console.ResetColor();

            Console.WriteLine("Ingrese los IDs de los instrumentos a eliminar (separados por comas):");
            Console.Write("IDs: ");
            string? input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("❌ No se proporcionaron IDs.");
                Console.ResetColor();
                return idsToDelete;
            }

            // Procesar los IDs ingresados
            var ids = input.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(id => id.Trim())
                          .Where(id => !string.IsNullOrWhiteSpace(id))
                          .ToList();

            if (!ids.Any())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("❌ No se proporcionaron IDs válidos.");
                Console.ResetColor();
                return idsToDelete;
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"IDs a eliminar ({ids.Count}):");
            for (int i = 0; i < ids.Count; i++)
            {
                Console.WriteLine($"  {i + 1}. {ids[i]}");
            }
            Console.ResetColor();

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("¿Confirma la eliminación? (S/N): ");
            Console.ResetColor();
            string? confirmation = Console.ReadLine();

            if (confirmation?.Equals("S", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                idsToDelete = ids;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✅ Eliminación confirmada. Procesando...");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("❌ Operación cancelada por el usuario.");
                Console.ResetColor();
            }

            return idsToDelete;
        }

        private static string BuildGetByIdRequest()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("╔═══════════════════════════════════════════════════════╗");
            Console.WriteLine("║           Búsqueda de Instrumento por ID             ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════╝");
            Console.ResetColor();

            Console.WriteLine("\nIngrese el ID del instrumento a buscar:");
            Console.Write("ID: ");
            string? instrumentId = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(instrumentId))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n❌ No se proporcionó ID.");
                Console.ResetColor();
                return string.Empty;
            }

            instrumentId = instrumentId.Trim();

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Buscando instrumento con ID: {instrumentId}");
            Console.ResetColor();

            return instrumentId;
        }

    }
}


