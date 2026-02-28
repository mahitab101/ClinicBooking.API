using System;

namespace ClinicBooking.API.Dtos.Patients;

public class CreatePatientDto
{
    public string FullName { get; set; }

    public string Email { get; set; }

    public string Phone { get; set; }

    public DateTime DateOfBirth { get; set; }
}
