using System;
using ClinicBooking.API.Dtos.Apoinment;
using ClinicBooking.API.Dtos.Patients;
using ClinicBooking.API.Entities;

namespace ClinicBooking.API.Mappings;

public static class PatientMappingExtentions
{
    public static PatientResponseDto ToDto(this Patient patient)
    {
        return new PatientResponseDto
        {
            Id = patient.Id,
            FullName = patient.FullName,
            Phone = patient.Phone,
            Email = patient.Email,
            DateOfBirth = patient.DateOfBirth
        };
    }

    public static Patient ToEntity(this CreatePatientDto patientDto)
    {
        return new Patient
        {
            FullName = patientDto.FullName,
            Phone = patientDto.Phone,
            Email = patientDto.Email,
            DateOfBirth = patientDto.DateOfBirth
        };
    }
    public static void UpdateEntity(this Patient patient, UpdatePatientDto dto)
    {
        patient.FullName = dto.FullName;
        patient.Phone = dto.Phone;
        patient.Email = dto.Email;
        patient.DateOfBirth = dto.DateOfBirth;
    }
}
