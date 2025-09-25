using FluentValidation;
using TooliRent.Application.DTOs;

namespace TooliRent.Application.Validation
{
    public sealed class BookingItemCreateDtoValidator : AbstractValidator<BookingItemCreateDto>
    {
        public BookingItemCreateDtoValidator()
        {
            RuleFor(x => x.ToolId).GreaterThan(0);
            RuleFor(x => x.Quantity).GreaterThan(0);
        }
    }

    public sealed class CreateBookingDtoValidator : AbstractValidator<CreateBookingDto>
    {
        public CreateBookingDtoValidator()
        {
            RuleFor(x => x.StartDate).NotEmpty();
            RuleFor(x => x.EndDate).NotEmpty();
            RuleFor(x => x).Must(x => x.StartDate.Date < x.EndDate.Date)
                .WithMessage("StartDate must be before EndDate.");

            RuleFor(x => x.Items)
                .NotNull().WithMessage("Items are required.")
                .NotEmpty().WithMessage("At least one item is required.");

            RuleForEach(x => x.Items).SetValidator(new BookingItemCreateDtoValidator());

            RuleFor(x => x.Items)
                .Must(items => items.Select(i => i.ToolId).Distinct().Count() == items.Count)
                .WithMessage("Duplicate tools are not allowed in a booking.");
        }
    }
}