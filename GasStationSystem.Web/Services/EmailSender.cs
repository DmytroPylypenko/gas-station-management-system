using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;

namespace GasStationSystem.Web.Services;

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Sends an email message using SMTP and MailKit.
    /// </summary>
    /// <param name="email">Recipient email address.</param>
    /// <param name="subject">Email subject.</param>
    /// <param name="htmlMessage">HTML content of the email body.</param>
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        try
        {
            // 1. Build the email message object
            var emailMessage = new MimeMessage();

            // Load SMTP credentials and sender information from configuration
            var settings = _configuration.GetSection("EmailSettings");
            var myEmail = settings["Mail"];
            var myPassword = settings["Password"];
            var myName = settings["DisplayName"];

            // Set the sender information
            emailMessage.From.Add(new MailboxAddress(myName, myEmail));

            // Set the recipient
            emailMessage.To.Add(new MailboxAddress("", email));

            // Set email subject and HTML body
            emailMessage.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlMessage
            };
            emailMessage.Body = bodyBuilder.ToMessageBody();

            // 2. Connect to the SMTP server and send the message
            using (var client = new SmtpClient())
            {
                client.CheckCertificateRevocation = false;

                var host = settings.GetValue<string>("Host")
                           ?? throw new InvalidOperationException("Email host is not configured.");

                var port = settings.GetValue<int?>("Port")
                           ?? throw new InvalidOperationException("Email port is not configured.");
                
                // Connect using STARTTLS
                await client.ConnectAsync(
                    host,
                    port,
                    MailKit.Security.SecureSocketOptions.StartTls);
                
                await client.AuthenticateAsync(myEmail, myPassword);
                
                await client.SendAsync(emailMessage);
                
                await client.DisconnectAsync(true);
            }

            _logger.LogInformation($"Email successfully sent to {email}");
        }
        catch (Exception ex)
        {
            // Log detailed error information and rethrow to propagate failure
            _logger.LogError(ex, "Error sending email");
            throw; 
        }
    }
}