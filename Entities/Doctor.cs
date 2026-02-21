using System;
using ClinicBooking.API.Common;

namespace ClinicBooking.API.Entities;

public class Doctor : SoftDeleteEntity
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public Guid SpecializationId { get; set; }

    public Specialization? Specialization { get; set; }
     public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
