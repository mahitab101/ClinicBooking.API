using System;

namespace ClinicBooking.API.Dtos.Patients;

public class PatientResponseDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; }

    public string Email { get; set; }

    public string Phone { get; set; }

    public DateTime DateOfBirth { get; set; }
    public int Age
    {
        get
        {
            var today = DateTime.Today;
            var age = today.Year - DateOfBirth.Year;

            if (DateOfBirth.Date > today.AddYears(-age))
                age--;

            return age;
        }
    }
}
