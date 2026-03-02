using System;

namespace ClinicBooking.API.Dtos.Apoinments;

public class UpdateAppointmentDto
{
    public DateTime AppointmentDate { get; set; }

    public Guid DoctorId { get; set; }
}
