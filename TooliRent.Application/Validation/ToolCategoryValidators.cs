using FluentValidation;
using TooliRent.Application.DTOs;

namespace TooliRent.Application.Validation
{
    public sealed class ToolCategoryCreateDtoValidator : AbstractValidator<ToolCategoryCreateDto>
    {
        public ToolCategoryCreateDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Description).MaximumLength(500);
        }
    }

    public sealed class ToolCategoryUpdateDtoValidator : AbstractValidator<ToolCategoryUpdateDto>
    {
        public ToolCategoryUpdateDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Description).MaximumLength(500);
        }
    }
}