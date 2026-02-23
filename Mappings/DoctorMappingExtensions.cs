using System;
using ClinicBooking.API.Dtos.Doctors;
using ClinicBooking.API.Entities;

namespace ClinicBooking.API.Mappings;

public static class DoctorMappingExtensions
{
    public static DoctorResponseDto ToDto(this Doctor doctor)
    {
        return new DoctorResponseDto
        {
            Id = doctor.Id,
            FullName = doctor.FullName,
            Email = doctor.Email,
            Phone = doctor.Phone,
            SpecializationId = doctor.SpecializationId,
            SpecializationName = doctor.Specialization?.Name
        };
    }

    public static Doctor ToEntity(this CreateDoctorDto dto)
    {
        return new Doctor
        {
            Id = Guid.NewGuid(),
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone,
            SpecializationId = dto.SpecializationId
        };
    }

    public static void UpdateEntity(this Doctor doctor, UpdateDoctorDto dto)
    {
        doctor.FullName = dto.FullName;
        doctor.Email = dto.Email;
        doctor.Phone = dto.Phone;
        doctor.SpecializationId = dto.SpecializationId;
    }
}
