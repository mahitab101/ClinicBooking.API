using System;

namespace ClinicBooking.API.Contracts;

public interface IAppointmentService
{
    Task<bool> Confirm(Guid id);
    Task<bool> Complete(Guid id);
    Task<bool> Cancel(Guid id);
}
