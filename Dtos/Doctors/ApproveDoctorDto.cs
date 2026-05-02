namespace ClinicBooking.API.Dtos.Doctors;

public class ApproveDoctorDto
{
    public Guid SpecializationId { get; set; }
    public decimal ConsultationFee { get; set; }
}
