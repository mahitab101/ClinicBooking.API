using System;
using ClinicBooking.API.Common;

namespace ClinicBooking.API.Entities;

public class DoctorSchedule : SoftDeleteEntity
{
    public Guid Id { get; set; }

    public Guid DoctorId { get; set; }
    public Doctor? Doctor { get; set; }

    public DayOfWeek DayOfWeek { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public int SlotDurationMinutes { get; set; } = 30;
}
