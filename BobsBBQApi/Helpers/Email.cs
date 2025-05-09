using BobsBBQApi.Helpers.Interfaces;
using BobsBBQApi.Services;
using MailKit.Net.Smtp;
using MimeKit;

namespace BobsBBQApi.Helpers;

public class Email : IEmail
{
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _smtpUser;
    private readonly string _smtpPass;

    public Email(string smtpServer, int smtpPort, string smtpUser, string smtpPass)
    {
        _smtpServer = smtpServer;
        _smtpPort = smtpPort;
        _smtpUser = smtpUser;
        _smtpPass = smtpPass;
    }

    public async Task SendSuccessfullAccountCreationEmail(string toEmail, string firstName)
    {
        MonitorService.Log.Information("Preparing to send successful account creation email to {@ToEmail} for user {@FirstName}",
            toEmail, firstName);
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress("Bobs BBQ", _smtpUser));
        email.To.Add(new MailboxAddress("Bobs BBQ", toEmail));
        email.Subject = "Successful User Registration";

        email.Body = new TextPart("html")
        {
            Text = $@"
                <p>Hello dear {firstName} !</p>
                <p>We are happy to inform you that you have been successfully registered at Bobs BBQ</p>
                <p>We hope to see you soon for a nice meal!</p>
                <p>Kind regards,</p>
                <p>Bobs BBQ</p>
            "
        };

        using var smtp = new SmtpClient();
        try
        {
            MonitorService.Log.Information("Attempting to connect to SMTP server {@SmtpServer} on port {@SmtpPort}", _smtpServer, _smtpPort);

            await smtp.ConnectAsync(_smtpServer, _smtpPort, true);
            await smtp.AuthenticateAsync(_smtpUser, _smtpPass);
            await smtp.SendAsync(email);
            MonitorService.Log.Information("Successfully sent account creation email to {@ToEmail}", toEmail);
        }
        catch (Exception ex)
        {
            MonitorService.Log.Error(ex, "Failed to send account creation email to {@ToEmail}", toEmail);
        }
        
        finally
        {
            await smtp.DisconnectAsync(true);
            MonitorService.Log.Information("Disconnected from SMTP server");
        }
    }
    
    
    public async Task SendSuccessfullTableReservationEmail(string toEmail, string firstName, 
        DateTime reservationDate, DateTime reservationTime)
    {
        MonitorService.Log.Information("Preparing to send successful table reservation email to {@ToEmail} for user {@FirstName}",
            toEmail, firstName);
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress("Bobs BBQ", _smtpUser));
        email.To.Add(new MailboxAddress("Bobs BBQ", toEmail));
        email.Subject = "BOBs BBQ Table Reservation";

        email.Body = new TextPart("html")
        {
            Text = $@"
                <p>Hello dear {firstName}!</p>
                <p>This is a confirmation mail for your reservation at Bobs BBQ</p>
                <p>Your reservation date it {reservationDate} at {reservationTime}</p>
                <p>We hope you will enjoy your meal</p>
                <p>Kind regards,</p>
                <p>Bobs BBQ</p>
            "
        };

        using var smtp = new SmtpClient();
        try
        {
            MonitorService.Log.Information("Attempting to connect to SMTP server {@SmtpServer} on port {@SmtpPort}", _smtpServer, _smtpPort);

            await smtp.ConnectAsync(_smtpServer, _smtpPort, true);
            await smtp.AuthenticateAsync(_smtpUser, _smtpPass);
            await smtp.SendAsync(email);
            MonitorService.Log.Information("Successfully sent account creation email to {@ToEmail}", toEmail);
        }
        catch (Exception ex)
        {
            MonitorService.Log.Error(ex, "Failed to send account creation email to {@ToEmail}", toEmail);
      
        }
        finally
        {
            await smtp.DisconnectAsync(true);
            MonitorService.Log.Information("Disconnected from SMTP server");
        }
    }
}