using System;
using System.Text.Json.Serialization;
using ClinicBooking.API.Enums;

namespace ClinicBooking.API.Dtos.Doctors;

public class DoctorResponseDto
{
    public Guid Id { get; set; }

    public string FullName { get; set; }

    public string Email { get; set; }

    public string Phone { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Gender Gender { get; set; }
    public DoctorLevel DoctorLevel { get; set; }
    public decimal ConsultationFee { get; set; }

    public Guid SpecializationId { get; set; }

    public string SpecializationName { get; set; }
}
