using System;

namespace ClinicBooking.API.Common;

public class SoftDeleteEntity : AuditableEntity
{
  public bool IsDeleted { get; set; } = false;
}
