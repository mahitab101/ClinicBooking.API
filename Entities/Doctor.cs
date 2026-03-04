using System;
using ClinicBooking.API.Common;
using ClinicBooking.API.Enums;

namespace ClinicBooking.API.Entities;

public class Doctor : SoftDeleteEntity
{
    public string FullName { get; set; }

    public string Email { get; set; }

    public string Phone { get; set; }
    public Gender Gender { get; set; }
    public DoctorLevel DoctorLevel { get; set; }
    public decimal ConsultationFee { get; set; }
    public Guid SpecializationId { get; set; }
    public Specialization Specialization { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<DoctorSchedule> Schedules { get; set; } = new List<DoctorSchedule>();
}
