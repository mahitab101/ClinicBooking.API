using ClinicBooking.API.Common;
using ClinicBooking.API.Entities;

public abstract class AuditableEntity : BaseEntity
{
    public Guid? CreatedBy { get; set; }
    public ApplicationUser? CreatedByUser { get; set; }

    public DateTime CreatedDate { get; set; }

    public Guid? LastModifiedBy { get; set; }
    public ApplicationUser? LastModifiedByUser { get; set; }

    public DateTime? LastModifiedDate { get; set; }
}