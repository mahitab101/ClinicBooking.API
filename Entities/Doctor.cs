using System;
using ClinicBooking.API.Common;

namespace ClinicBooking.API.Entities;

public class Doctor : SoftDeleteEntity
{
    public string FullName { get; set; }

    public string Email { get; set; }

    public string Phone { get; set; }

    public Guid SpecializationId { get; set; }

    public Specialization? Specialization { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
