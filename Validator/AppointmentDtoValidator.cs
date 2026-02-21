using System;
using ClinicBooking.API.Dtos.Apoinment;
using FluentValidation;

namespace ClinicBooking.API.Validator;

public class AppointmentDtoValidator : AbstractValidator<CreateAppointmentDto>
{
    public AppointmentDtoValidator()
    {
        RuleFor(a => a.DoctorId).NotEmpty().WithMessage("Doctor is required");
        RuleFor(a => a.PatientId).NotEmpty().WithMessage("Patient is required");
        RuleFor(a => a.AppointmentDate).NotEmpty()
                                       .WithMessage("Date is required")
                                       .GreaterThan(DateTime.UtcNow)
                                       .WithMessage("Appointment must be in the future.");
    }
}
