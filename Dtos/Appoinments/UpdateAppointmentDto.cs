using System;

namespace ClinicBooking.API.Dtos.Appoinments;

public class UpdateAppointmentDto
{
    public DateTime AppointmentDate { get; set; }

    public Guid DoctorId { get; set; }
}
