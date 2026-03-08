using System;
using ClinicBooking.API.Dtos.DoctorSchedules;
using ClinicBooking.API.Entities;

namespace ClinicBooking.API.Mappings;

public static class DoctorScheduleMappingExtensions
{
    public static DoctorScheduleResponseDto ToDto(this DoctorSchedule schedule)
    {
        return new DoctorScheduleResponseDto
        {
            Id = schedule.Id,
            DoctorId = schedule.DoctorId,
            DayOfWeek = schedule.DayOfWeek,
            StartTime = schedule.StartTime,
            EndTime = schedule.EndTime,
            SlotDurationMinutes = schedule.SlotDurationMinutes
        };
    }

    public static DoctorSchedule ToEntity(this CreateDoctorScheduleDto dto)
    {
        return new DoctorSchedule
        {
            Id = Guid.NewGuid(),
            DoctorId = dto.DoctorId,
            DayOfWeek = dto.DayOfWeek,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            SlotDurationMinutes = dto.SlotDurationMinutes
        };
    }

    public static void UpdateEntity(this DoctorSchedule entity, UpdateDoctorScheduleDto dto)
    {
        entity.DayOfWeek = dto.DayOfWeek;
        entity.StartTime = dto.StartTime;
        entity.EndTime = dto.EndTime;
        entity.SlotDurationMinutes = dto.SlotDurationMinutes;
    }
}
