using System;
using ClinicBooking.API.Dtos.Doctors;
using FluentValidation;

namespace ClinicBooking.API.Validator;

public class DoctorDtoValidator : AbstractValidator<CreateDoctorDto>
{
    public DoctorDtoValidator()
    {
        RuleFor(x => x.FullName)
                    .NotEmpty()
                    .MinimumLength(3)
                    .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Phone)
            .NotEmpty()
            .Matches(@"^971\s\d{9}$")
            .WithMessage("Invalid phone number");

        RuleFor(x => x.SpecializationId)
            .NotEmpty()
            .WithMessage("Specialization is required");
    }
}
