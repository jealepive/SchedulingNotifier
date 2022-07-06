using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;

namespace SchedulingNotifier.Core;

public interface IEmailService
{
    Task Send(string from, string to, string cc, string subject, string html);
}

public class EmailService : IEmailService
{
    private readonly AppSettings _appSettings;

    public EmailService(AppSettings appSettings)
    {
        _appSettings = appSettings;
    }

    public async Task Send(string from, string to, string cc, string subject, string html)
    {
        // create message
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(from));
        email.To.Add(MailboxAddress.Parse(to));
        email.Cc.Add(MailboxAddress.Parse(cc));
        email.Subject = subject;
        email.Body = new TextPart(TextFormat.Html) { Text = html };

        // send email
        using var smtp = new SmtpClient();
        smtp.Connect(_appSettings.SmtpHost, _appSettings.SmtpPort, useSsl: true);
        smtp.Authenticate(_appSettings.SmtpUser, _appSettings.SmtpPass);
        await smtp.SendAsync(email);
        smtp.Disconnect(true);
    }
}