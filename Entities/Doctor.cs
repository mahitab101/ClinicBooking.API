using ClinicBooking.API.Common;
using ClinicBooking.API.Enums;

namespace ClinicBooking.API.Entities;

public class Doctor : SoftDeleteEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public DoctorLevel DoctorLevel { get; set; }
    public decimal ConsultationFee { get; set; }

    // Assigned by Admin after approval
    public Guid? SpecializationId { get; set; }
    public Specialization? Specialization { get; set; }

    // Linked to ApplicationUser
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    // Certificate uploaded during registration
    public string? CertificateUrl { get; set; }

    // Approval workflow
    public bool IsApproved { get; set; } = false;
    public bool IsActive { get; set; } = false;
    public bool IsRejected { get; set; } = false;
    public string? RejectionReason { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<DoctorSchedule> Schedules { get; set; } = new List<DoctorSchedule>();
}
