using System;

namespace ClinicBooking.API.Dtos.Doctors;

public class DoctorResponseDto
{
    public Guid Id { get; set; }

    public string FullName { get; set; }

    public string Email { get; set; }

    public string Phone { get; set; }

    public Guid SpecializationId { get; set; }

    public string SpecializationName { get; set; }
}
