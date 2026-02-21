using System;
using ClinicBooking.API.Common;

namespace ClinicBooking.API.Entities;

public class Patient : SoftDeleteEntity
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
