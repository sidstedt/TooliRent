using FluentValidation;
using TooliRent.Application.DTOs;
using TooliRent.Domain.Enums;

namespace TooliRent.Application.Validation
{
    public sealed class ToolCreateDtoValidator : AbstractValidator<ToolCreateDto>
    {
        public ToolCreateDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
            RuleFor(x => x.PricePerDay).InclusiveBetween(0m, 100000m)
                .PrecisionScale(18, 2, false)
                .WithMessage("Price per day may have at most 2 decimal.");
            RuleFor(x => x.QuantityAvailable).GreaterThanOrEqualTo(0);
            RuleFor(x => x.CategoryId).GreaterThan(0);
        }
    }

    public sealed class ToolUpdateDtoValidator : AbstractValidator<ToolUpdateDto>
    {
        public ToolUpdateDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
            RuleFor(x => x.PricePerDay).InclusiveBetween(0m, 100000m)
                .PrecisionScale(18, 2, false)
                .WithMessage("Price per day may have at most 2 decimal.");
            RuleFor(x => x.QuantityAvailable).GreaterThanOrEqualTo(0);
            RuleFor(x => x.CategoryId).GreaterThan(0);
            RuleFor(x => x.Status).Must(v => Enum.IsDefined(typeof(ToolStatus), v))
                .WithMessage("Status must be valid");
        }
    }
}
