using System;

namespace ClinicBooking.API.Dtos.DoctorSchedules;

public class DoctorScheduleResponseDto
{
    public Guid Id { get; set; }

    public Guid DoctorId { get; set; }

    public DayOfWeek DayOfWeek { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public int SlotDurationMinutes { get; set; }
}
