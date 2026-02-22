using System;
using ClinicBooking.API.Dtos.Specializations;
using ClinicBooking.API.Entities;

namespace ClinicBooking.API.Mappings;

public static class SpecializationMappingExtensions
{
    public static SpecializationDto ToDto(this Specialization entity)
    {
        return new SpecializationDto
        {
            Id = entity.Id,
            Name = entity.Name
        };
    }

    public static Specialization ToEntity(this CreateSpecializationDto dto)
    {
        return new Specialization
        {
            Id = Guid.NewGuid(),
            Name = dto.Name
        };
    }

}
