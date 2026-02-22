using System;
using ClinicBooking.API.Dtos.Specializations;
using FluentValidation;

namespace ClinicBooking.API.Validator;

public class SpecializationDTOValidator : AbstractValidator<CreateSpecializationDto>
{
    public SpecializationDTOValidator()
    {
        RuleFor(x => x.Name)
                    .NotEmpty().WithMessage("Specialization name is required")
                    .MinimumLength(3).WithMessage("Name must be at least 3 characters")
                    .MaximumLength(100).WithMessage("Name cannot exceed 100 characters")
                    .Matches(@"^[a-zA-Z\s]+$")
                    .WithMessage("Name must contain letters only");
    }
}
