using System;
using ClinicBooking.API.Common;
using ClinicBooking.API.Enums;

namespace ClinicBooking.API.Entities;

public class Appointment :AuditableEntity
{
    public DateTime AppointmentDate { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    public Guid DoctorId { get; set; }
    public Doctor? Doctor { get; set; }

    public Guid PatientId { get; set; }
    public Patient? Patient { get; set; }
    public MedicalRecord? MedicalRecord { get; set; }

}

