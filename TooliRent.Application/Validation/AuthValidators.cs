using FluentValidation;
using TooliRent.Application.DTOs;

namespace TooliRent.Application.Validation
{
    public sealed class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {
        public RegisterDtoValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
            RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(100);
        }
    }

    public sealed class LoginDtoValidator : AbstractValidator<LoginDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty();
        }
    }

    public sealed class RefreshTokenDtoValidator : AbstractValidator<RefreshTokenDto>
    {
        public RefreshTokenDtoValidator()
        {
            RuleFor(x => x.RefreshToken).NotEmpty();
        }
    }
}