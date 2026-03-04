using System;
using ClinicBooking.API.Enums;

namespace ClinicBooking.API.Dtos.Doctors;

public class CreateDoctorDto
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public Gender Gender { get; set; }
    public DoctorLevel DoctorLevel { get; set; }
    public decimal ConsultationFee { get; set; }
    public Guid SpecializationId { get; set; }
}
