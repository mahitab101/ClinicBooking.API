using ClinicBooking.API.Common;

namespace ClinicBooking.API.Entities;

public class Patient : SoftDeleteEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }

    // Linked to ApplicationUser
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
