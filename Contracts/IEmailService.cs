namespace ClinicBooking.API.Contracts;

public interface IEmailService
{
    Task SendAsync(string toEmail, string toName, string subject, string htmlBody);
    Task SendDoctorPendingApprovalToAdminAsync(string doctorName, string doctorEmail);
    Task SendDoctorApprovedAsync(string doctorEmail, string doctorName);
    Task SendDoctorRejectedAsync(string doctorEmail, string doctorName, string reason);
    Task SendAppointmentBookedAsync(string patientEmail, string patientName, string doctorName, DateTime appointmentDate);
}
