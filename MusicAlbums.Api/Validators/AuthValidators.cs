using FluentValidation;
using MusicAlbums.Api.Dtos;

namespace MusicAlbums.Api.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().Length(3, 50)
            .Matches("^[a-zA-Z0-9_]+$")
            .WithMessage("Faqat lotin harflari, raqamlar va _ ishlatiladi.");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).MaximumLength(100);
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}