using AutoMapper;
using TiendaMusica.Application.Dtos;
using TiendaMusica.Application.UseCases.Instruments;
using TiendaMusica.Infrastructure.Entrypoint.Cli.Dtos;

namespace TiendaMusica.Infrastructure.Entrypoint.Cli.Commands
{
    public class InstrumentsCommand
    {
        private readonly IInstrumentUseCase _instrumentUseCase;
        private readonly IMapper _mapper;

        public InstrumentsCommand(
            IInstrumentUseCase instrumentUseCase,
            IMapper mapper
            )
        {
            _instrumentUseCase = instrumentUseCase;
            _mapper = mapper;
        }

        public async Task GetAllAsync()
        {
            var instrumentsResult = await _instrumentUseCase.GetAllAsync();

            if (instrumentsResult.HasErrors)
            {
                Console.ForegroundColor = ConsoleColor.Red;

                instrumentsResult.Errors.ForEach(error =>
                {
                    Console.WriteLine($"Error Code: {error.ErrorCode}, Message: {error.Message}");
                });

                Console.ResetColor();
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
            }
        }

        public async Task CreateAsync(InstrumentCreateCliRequest command)
        {
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
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Id: {instrumentCreateResult.Result.Id}, Name: {instrumentCreateResult.Result.Name}, Type: {instrumentCreateResult.Result.Type}");
                Console.ResetColor();
            }
        }
    }
}
