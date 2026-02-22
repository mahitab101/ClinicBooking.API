using System;
using ClinicBooking.API.Entities;

namespace ClinicBooking.API.Contracts;

public interface IUnitOfWork : IDisposable
{
    IBaseRepository<Specialization> Specializations { get; }
    IBaseRepository<Doctor> Doctors { get; }
    IBaseRepository<Patient> Patients { get; }
    IBaseRepository<Appointment> Appointments { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
