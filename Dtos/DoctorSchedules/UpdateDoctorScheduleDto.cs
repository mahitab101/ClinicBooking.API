using System;

namespace ClinicBooking.API.Dtos.DoctorSchedules;

public class UpdateDoctorScheduleDto
{
    public DayOfWeek DayOfWeek { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public int SlotDurationMinutes { get; set; }
}
