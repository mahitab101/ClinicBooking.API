using System;
using ClinicBooking.API.Dtos.Doctors;
using FluentValidation;

namespace ClinicBooking.API.Validator;

public class DoctorDtoValidator : AbstractValidator<CreateDoctorDto>
{
    public DoctorDtoValidator()
    {
        RuleFor(d => d.FullName)
                    .NotEmpty()
                    .WithMessage("Doctor name is required")
                    .MinimumLength(3)
                    .MaximumLength(100);

        RuleFor(d => d.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(d => d.Phone)
            .NotEmpty()
            .Matches(@"^971\s\d{9}$")
            .WithMessage("Invalid phone number");

        RuleFor(d => d.SpecializationId)
            .NotEmpty()
            .WithMessage("Specialization is required");

        RuleFor(d => d.ConsultationFee)
            .GreaterThan(0)
            .WithMessage("Consultation fee must be greater than zero");
    }
}
