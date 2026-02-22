using System;
using ClinicBooking.API.Contracts;
using ClinicBooking.API.Data;
using ClinicBooking.API.Entities;

namespace ClinicBooking.API.Repository;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _dbContext;

    public IBaseRepository<Specialization> Specializations { get; }

    public IBaseRepository<Doctor> Doctors { get; }

    public IBaseRepository<Patient> Patients { get; }

    public IBaseRepository<Appointment> Appointments { get; }

    public UnitOfWork(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;

        Specializations = new BaseRepository<Specialization>(_dbContext);
        Doctors = new BaseRepository<Doctor>(_dbContext);
        Patients = new BaseRepository<Patient>(_dbContext);
        Appointments = new BaseRepository<Appointment>(_dbContext);
    }
    public void Dispose()
    {
        _dbContext.Dispose();
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
