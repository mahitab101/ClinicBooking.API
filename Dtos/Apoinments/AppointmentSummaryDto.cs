using System;
using ClinicBooking.API.Enums;

namespace ClinicBooking.API.Dtos.Apoinments;

public class AppointmentSummaryDto
{
    public Guid Id { get; set; }

    public DateTime AppointmentDate { get; set; }

    public AppointmentStatus Status { get; set; }

    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;

    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
}
