using Microsoft.Extensions.DependencyInjection;
using TiendaMusica.Domain.Enums;
using TiendaMusica.Infrastructure.Entrypoint.Cli.Commands;
using TiendaMusica.Infrastructure.Entrypoint.Cli.Constants;
using TiendaMusica.Infrastructure.Entrypoint.Cli.Dtos;

namespace TiendaMusica.Infrastructure.Entrypoint.Cli
{
    public static class MainExecute
    {
        public static async Task ExecuteAsync(string[] args, IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var provider = scope.ServiceProvider;

            var command = args[0].ToLower();
            var instrumentsCommand = provider.GetRequiredService<InstrumentsCommand>();

            switch (command)
            {
                case InstrumentParameters.List:
                    await instrumentsCommand.GetAllAsync();
                    break;
                case InstrumentParameters.Add:

                    if (!Enum.TryParse<InstrumentType>(args[3], true, out var instrumentType))
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"⚠️ '{args[3]}' no es un tipo de instrumento válido.");
                        Console.WriteLine("Opciones válidas: Wind, Stringed, Keyboard");
                        Console.ResetColor();
                        return;
                    }

                    var createCommand = new InstrumentCreateCliRequest(Name: args[1],
                        Description: args[2],
                        Type: instrumentType,
                        Price: decimal.Parse(args[4]),
                        Stock: int.Parse(args[5]));

                    await instrumentsCommand.CreateAsync(createCommand);
                    break;
                    default:
                    Console.WriteLine($"Comando desconocido: {command}");
                    break;
            }
        }
    }
}
