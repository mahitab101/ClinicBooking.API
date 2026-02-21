using System;
using ClinicBooking.API.Common;

namespace ClinicBooking.API.Entities;

public class MedicalRecord : AuditableEntity
{
    public Guid AppointmentId { get; set; }

    public Appointment? Appointment { get; set; }

    public string Diagnosis { get; set; } = string.Empty;

    public string? Prescription { get; set; }

    public string? Notes { get; set; }
}
