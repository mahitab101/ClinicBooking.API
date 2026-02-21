using System;
using ClinicBooking.API.Dtos.Specialization;
using FluentValidation;

namespace ClinicBooking.API.Validator;

public class SpecializationDTOValidator : AbstractValidator<CreateSpecializationDto>
{
    public SpecializationDTOValidator()
    {
        RuleFor(a => a.Name).NotEmpty().WithMessage("Specialization name is required");
    }
}
