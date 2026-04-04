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

                        var createCommand = BuildCliRequest(args);
                        await instrumentsCommand.CreateAsync(createCommand);
                        Log.Information("Agregando nuevo instrumento: {Name}", createCommand.Name);

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

        private static InstrumentCreateCliRequest BuildCliRequest(string[] args)
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
            Console.WriteLine("  instrument-list           - Lista todos los instrumentos");
            Console.WriteLine("  instrument-add            - Agrega un nuevo instrumento");
            Console.WriteLine("Ejemplo:");
            Console.WriteLine("  instrument-add");
            Console.ResetColor();
        }

        private static GetAllInstrumentQuery? BuildGetAllQuery()
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

                return new GetAllInstrumentQuery(
                    PageSize: int.Parse(pageSizeInput),
                    Search: search,
                    PageNumber: int.Parse(pageNumberInput)
                    );
            }

            Console.ResetColor();

            return null;
        }
    }
}


