namespace ClinicBooking.API.Dtos.Doctors;

public class RejectDoctorDto
{
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// true  = permanently delete the user account (fake/fraudulent certificate)
    /// false = soft reject, doctor can resubmit documents
    /// </summary>
    public bool Permanent { get; set; } = false;
}
