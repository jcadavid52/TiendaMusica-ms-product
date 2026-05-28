using FluentValidation;
using TiendaMusica.Infrastructure.Entrypoint.Rest.Dtos;

namespace TiendaMusica.Infrastructure.Entrypoint.Rest.Validators
{
    internal class InstrumentRequestValidator : AbstractValidator<InstrumentCreateRequest>
    {
        public InstrumentRequestValidator()
        {
            RuleFor(request => request.Name)
            .NotEmpty()
            .WithMessage("El nombre es obligatorio.")
            .Length(2, 50);

            RuleFor(request => request.Description)
            .NotEmpty()
            .WithMessage("La descripción es obligatoria.")
            .Length(2, 150);

            RuleFor(request => request.Type)
            .NotNull()
            .WithMessage("El tipo de instrumento es obligatorio.")
            .IsInEnum()
            .WithMessage("El valor enviado no es un tipo de instrumento válido.");

            RuleFor(request => request.Price)
                .NotEmpty()
                .WithMessage("El precio es obligatorio.")
                .GreaterThan(0)
                .WithMessage("El precio debe ser mayor que cero.");

            RuleFor(request => request.Stock)
                .NotNull().WithMessage("El stock es obligatorio")
                .GreaterThanOrEqualTo(0)
                .WithMessage("El stock no puede ser un número negativo.");

        }
    }
}
