using System;

namespace ClinicBooking.API.Dtos.Doctors;

public class CreateDoctorDto
{
    public string FullName { get; set; }

    public string Email { get; set; }

    public string Phone { get; set; }

    public Guid SpecializationId { get; set; }
}
