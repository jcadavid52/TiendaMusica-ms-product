using FluentValidation;
using TiendaMusica.Infrastructure.Entrypoint.Rest.Dtos;

namespace TiendaMusica.Infrastructure.Entrypoint.Rest.Validators
{
    internal class InstrumentUpdateRequestValidator : AbstractValidator<InstrumentUpdateRequest>
    {
        public InstrumentUpdateRequestValidator()
        {
            RuleFor(request => request.Id)
                .NotEmpty()
                .WithMessage("El ID es obligatorio.")
                .NotNull()
                .WithMessage("El ID es obligatorio.");

            RuleFor(request => request.Name)
                .NotEmpty()
                .WithMessage("El nombre es obligatorio.")
                .Length(2, 50)
                .WithMessage("El nombre debe tener entre 2 y 50 caracteres.");

            RuleFor(request => request.Description)
                .NotEmpty()
                .WithMessage("La descripción es obligatoria.")
                .Length(2, 150)
                .WithMessage("La descripción debe tener entre 2 y 150 caracteres.");

            RuleFor(request => request.Type)
                .NotNull()
                .WithMessage("El tipo de instrumento es obligatorio.")
                .IsInEnum()
                .WithMessage("El valor enviado no es un tipo de instrumento válido.");
        }
    }
}
