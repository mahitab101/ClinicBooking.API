using System;
using ClinicBooking.API.Common;
using ClinicBooking.API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.API.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    { }

    public DbSet<Specialization> Specializations { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<MedicalRecord> MedicalRecords { get; set; }
    public DbSet<DoctorSchedule> DoctorSchedules { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Appointment>().HasIndex(a => new { a.DoctorId, a.AppointmentDate }).IsUnique();
        builder.Entity<MedicalRecord>().HasIndex(m => m.AppointmentId).IsUnique();

        // Fix decimal precision warning
        builder.Entity<Doctor>()
               .Property(d => d.ConsultationFee)
               .HasPrecision(18, 2);

        builder.Entity<Doctor>()
               .Property(d => d.Gender)
               .HasConversion<string>();

        builder.Entity<DoctorSchedule>()
               .HasOne(s => s.Doctor)
               .WithMany(d => d.Schedules)
               .HasForeignKey(s => s.DoctorId);

        // Break cascade cycles
        builder.Entity<Doctor>()
               .HasOne(d => d.User)
               .WithMany()
               .HasForeignKey(d => d.UserId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Patient>()
               .HasOne(p => p.User)
               .WithMany()
               .HasForeignKey(p => p.UserId)
               .OnDelete(DeleteBehavior.NoAction);

        // Fix "required end of relationship filtered out" warning
        // Make Appointment -> Doctor navigation optional so soft-deleted doctors don't break queries
        builder.Entity<Appointment>()
               .HasOne(a => a.Doctor)
               .WithMany(d => d.Appointments)
               .HasForeignKey(a => a.DoctorId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Appointment>()
               .HasOne(a => a.Patient)
               .WithMany(p => p.Appointments)
               .HasForeignKey(a => a.PatientId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.NoAction);

        // Global Filters
        builder.Entity<Doctor>().HasQueryFilter(d => !d.IsDeleted);
        builder.Entity<Patient>().HasQueryFilter(p => !p.IsDeleted);
        builder.Entity<Specialization>().HasQueryFilter(s => !s.IsDeleted);
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
