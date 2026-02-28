using System;
using ClinicBooking.API.Dtos.Patients;
using FluentValidation;

namespace ClinicBooking.API.Validator;

public class PatientDtoValidator : AbstractValidator<CreatePatientDto>
{
    public PatientDtoValidator()
    {
        RuleFor(p => p.FullName)
            .NotEmpty()
            .WithMessage("Patient name is required")
            .MaximumLength(150);

        RuleFor(p => p.Email)
            .EmailAddress()
            .When(p => !string.IsNullOrWhiteSpace(p.Email))
            .WithMessage("Invalid email format");

        RuleFor(p => p.Phone)
            .NotEmpty()
            .WithMessage("Phone number is required")
            .Matches(@"^971\d{9}$")
            .WithMessage("Phone must be in format 971XXXXXXXXX");

        RuleFor(p => p.DateOfBirth)
            .NotEmpty()
            .WithMessage("Date of birth is required")
            .LessThan(DateTime.Today)
            .WithMessage("Date of birth must be in the past");
    }
}
