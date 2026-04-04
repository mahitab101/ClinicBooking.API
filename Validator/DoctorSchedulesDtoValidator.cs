using System;
using ClinicBooking.API.Dtos.DoctorSchedules;
using FluentValidation;

namespace ClinicBooking.API.Validator;

public class DoctorSchedulesDtoValidator : AbstractValidator<CreateDoctorScheduleDto>
{
    public DoctorSchedulesDtoValidator()
    {
        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime)
            .WithMessage("EndTime must be greater than StartTime");

        RuleFor(x => x.SlotDurationMinutes)
            .GreaterThan(0)
            .WithMessage("Slot duration must be greater than zero");
    }
}
