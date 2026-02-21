using System;
using ClinicBooking.API.Common;

namespace ClinicBooking.API.Entities;

public class Specialization : SoftDeleteEntity
{
    public string Name { get; set; } = string.Empty;

}

