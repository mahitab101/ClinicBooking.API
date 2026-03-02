using System;
using ClinicBooking.API.Enums;

namespace ClinicBooking.API.Dtos.Apoinments;

public class AppointmentResponseDto
{
    public Guid Id { get; set; }

    public DateTime AppointmentDate { get; set; }

    public AppointmentStatus Status { get; set; }

    public Guid DoctorId { get; set; }

    public Guid PatientId { get; set; }
}
