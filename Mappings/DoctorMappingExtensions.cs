using System;
using ClinicBooking.API.Dtos.Doctors;
using ClinicBooking.API.Entities;
using ClinicBooking.API.Enums;

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
            SpecializationName = doctor.Specialization?.Name,
            Gender = doctor.Gender,
            DoctorLevel = doctor.DoctorLevel,
            ConsultationFee = doctor.ConsultationFee

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
            Gender = dto.Gender,
            DoctorLevel = dto.DoctorLevel,
            ConsultationFee = dto.ConsultationFee,
            SpecializationId = dto.SpecializationId
        };
    }

    public static void UpdateEntity(this Doctor doctor, UpdateDoctorDto dto)
    {
        doctor.FullName = dto.FullName;
        doctor.Email = dto.Email;
        doctor.Phone = dto.Phone;
        doctor.Gender = dto.Gender;
        doctor.DoctorLevel = dto.DoctorLevel;
        doctor.ConsultationFee = dto.ConsultationFee;
        doctor.SpecializationId = dto.SpecializationId;
    }
}
