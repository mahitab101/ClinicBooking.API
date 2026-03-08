using System;
using ClinicBooking.API.Contracts;
using ClinicBooking.API.Enums;

namespace ClinicBooking.API.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IUnitOfWork _unitOfWork;

    public AppointmentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Confirm(Guid id)
    {
        var appointment = await _unitOfWork.Appointments.GetByIdAsync(id);
        if (appointment == null) return false;

        if (appointment.Status != AppointmentStatus.Pending)
            throw new InvalidOperationException();

        appointment.Status = AppointmentStatus.Confirmed;

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> Complete(Guid id)
    {
        var appointment = await _unitOfWork.Appointments.GetByIdAsync(id);
        if (appointment == null) return false;

        if (appointment.Status != AppointmentStatus.Confirmed)
            throw new InvalidOperationException();

        appointment.Status = AppointmentStatus.Completed;

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> Cancel(Guid id)
    {
        var appointment = await _unitOfWork.Appointments.GetByIdAsync(id);
        if (appointment == null) return false;

        if (appointment.Status == AppointmentStatus.Completed)
            throw new InvalidOperationException();

        appointment.Status = AppointmentStatus.Cancelled;

        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
