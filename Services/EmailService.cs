using ClinicBooking.API.Contracts;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace ClinicBooking.API.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    // ─── Core send ───────────────────────────────────────────────────────────
    public async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        var emailSettings = _configuration.GetSection("Email");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            emailSettings["SenderName"] ?? "ClinicBooking",
            emailSettings["SenderEmail"]));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(
                emailSettings["Host"],
                int.Parse(emailSettings["Port"] ?? "587"),
                SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(
                emailSettings["Username"],
                emailSettings["Password"]);

            await client.SendAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            // Don't throw — email failure should never break the main flow
        }
        finally
        {
            await client.DisconnectAsync(true);
        }
    }

    // ─── Notification templates ───────────────────────────────────────────────

    public async Task SendDoctorPendingApprovalToAdminAsync(string doctorName, string doctorEmail)
    {
        var adminEmail = _configuration["Email:AdminEmail"] ?? "";
        var subject = "New Doctor Pending Approval";
        var body = $"""
            <h2>New Doctor Registration</h2>
            <p>A new doctor has registered and is awaiting your approval:</p>
            <ul>
                <li><strong>Name:</strong> {doctorName}</li>
                <li><strong>Email:</strong> {doctorEmail}</li>
            </ul>
            <p>Please log in to the admin panel to review their certificate and approve or reject their account.</p>
        """;

        await SendAsync(adminEmail, "Admin", subject, body);
    }

    public async Task SendDoctorApprovedAsync(string doctorEmail, string doctorName)
    {
        var subject = "Your ClinicBooking Account Has Been Approved!";
        var body = $"""
            <h2>Welcome to ClinicBooking, {doctorName}!</h2>
            <p>Great news — your account has been approved by our admin team.</p>
            <p>You can now log in and start managing your schedule and appointments.</p>
            <br/>
            <p>Best regards,<br/>The ClinicBooking Team</p>
        """;

        await SendAsync(doctorEmail, doctorName, subject, body);
    }

    public async Task SendDoctorRejectedAsync(string doctorEmail, string doctorName, string reason)
    {
        var subject = "Update on Your ClinicBooking Application";
        var body = $"""
            <h2>Hello {doctorName},</h2>
            <p>We reviewed your application and unfortunately we were unable to approve your account at this time.</p>
            <p><strong>Reason:</strong> {reason}</p>
            <p>If you believe this is a mistake or would like to resubmit with updated documents, please contact our support team.</p>
            <br/>
            <p>Best regards,<br/>The ClinicBooking Team</p>
        """;

        await SendAsync(doctorEmail, doctorName, subject, body);
    }

    public async Task SendAppointmentBookedAsync(
        string patientEmail, string patientName,
        string doctorName, DateTime appointmentDate)
    {
        var subject = "Appointment Confirmation";
        var body = $"""
            <h2>Appointment Confirmed</h2>
            <p>Dear {patientName},</p>
            <p>Your appointment has been successfully booked:</p>
            <ul>
                <li><strong>Doctor:</strong> {doctorName}</li>
                <li><strong>Date & Time:</strong> {appointmentDate:dddd, MMMM d yyyy} at {appointmentDate:h:mm tt}</li>
            </ul>
            <p>Please arrive 10 minutes early. If you need to cancel, please do so at least 24 hours in advance.</p>
            <br/>
            <p>Best regards,<br/>The ClinicBooking Team</p>
        """;

        await SendAsync(patientEmail, patientName, subject, body);
    }
}
