using ClinicBooking.API.Dtos.Apoinment;
using ClinicBooking.API.Dtos.Apoinments;
using ClinicBooking.API.Entities;
using ClinicBooking.API.Enums;

public static class AppointmentMappingExtensions
{
    public static AppointmentResponseDto ToDto(this Appointment appointment)
    {
        return new AppointmentResponseDto
        {
            Id = appointment.Id,
            AppointmentDate = appointment.AppointmentDate,
            Status = appointment.Status,
            DoctorId = appointment.DoctorId,
            PatientId = appointment.PatientId
        };
    }

    public static Appointment ToEntity(this CreateAppointmentDto dto)
    {
        return new Appointment
        {
            Id = Guid.NewGuid(),
            AppointmentDate = dto.AppointmentDate,
            DoctorId = dto.DoctorId,
            PatientId = dto.PatientId,
            Status = AppointmentStatus.Pending
        };
    }

    public static void UpdateEntity(this Appointment appointment, UpdateAppointmentDto dto)
    {
        appointment.AppointmentDate = dto.AppointmentDate;
        appointment.DoctorId = dto.DoctorId;
    }
}