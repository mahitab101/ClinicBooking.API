
namespace ClinicBooking.API.Dtos.Apoinment;

public class CreateAppointmentDto
{
    public DateTime AppointmentDate { get; set; }
    public Guid DoctorId { get; set; }
    public Guid PatientId { get; set; }
}
