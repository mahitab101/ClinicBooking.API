using System;
using ClinicBooking.API.Common;
using ClinicBooking.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    { }

    public DbSet<Specialization> Specializations { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<MedicalRecord> MedicalRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Appointment>().HasIndex(a => new { a.DoctorId, a.AppointmentDate }).IsUnique();
        builder.Entity<MedicalRecord>().HasIndex(m => m.AppointmentId).IsUnique();

        builder.Entity<DoctorSchedule>()
               .HasOne(s => s.Doctor)
               .WithMany(d => d.Schedules)
               .HasForeignKey(s => s.DoctorId);

        builder.Entity<Doctor>()
               .Property(d => d.Gender)
               .HasConversion<string>();

        // Globel Filter
        builder.Entity<Doctor>().HasQueryFilter(d => !d.IsDeleted);
        builder.Entity<Patient>().HasQueryFilter(d => !d.IsDeleted);
        builder.Entity<Specialization>().HasQueryFilter(d => !d.IsDeleted);
        builder.Entity<DoctorSchedule>().HasQueryFilter(s => !s.IsDeleted);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries<AuditableEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedDate = DateTime.UtcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.LastModifiedDate = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

}
