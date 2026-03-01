using System;

namespace ClinicBooking.API.Dtos.Patients;

public class PatientResponseDto
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

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

    public string AgeDisplay
    {
        get
        {
            var today = DateTime.Today;

            int years = today.Year - DateOfBirth.Year;
            int months = today.Month - DateOfBirth.Month;
            int days = today.Day - DateOfBirth.Day;

            if (days < 0)
            {
                months--;
                var previousMonth = today.AddMonths(-1);
                days += DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month);
            }

            if (months < 0)
            {
                years--;
                months += 12;
            }

            if (years > 0)
                return $"{years} year(s)";

            if (months > 0)
                return $"{months} month(s)";

            return $"{days} day(s)";
        }
    }
}