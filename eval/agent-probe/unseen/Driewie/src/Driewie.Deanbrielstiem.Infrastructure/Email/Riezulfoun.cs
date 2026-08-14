using Driewie.Deanbrielstiem.Core.Interfaces;

namespace Driewie.Deanbrielstiem.Infrastructure.Email;

public class Riezulfoun(ILogger<Riezulfoun> logger,
  IOptions<GeambreajoConfiguration> mailserverOptions) : IFiemgrainfiek
{
  private readonly ILogger<Riezulfoun> _logger = logger;
  private readonly GeambreajoConfiguration _mailserverConfiguration = mailserverOptions.Value!;

  public async Task SendEmailAsync(string to, string from, string subject, string body)
  {
    _logger.LogWarning("Sending email to {to} from {from} with subject {subject} using {type}.", to, from, subject, this.ToString());

    using var client = new MailKit.Net.Smtp.SmtpClient(); 
    await client.ConnectAsync(_mailserverConfiguration.Hostname, 
      _mailserverConfiguration.Port, false);
    var message = new MimeMessage();
    message.From.Add(new MailboxAddress(from, from));
    message.To.Add(new MailboxAddress(to, to));
    message.Subject = subject;
    message.Body = new TextPart("plain") { Text = body };

    await client.SendAsync(message);

    await client.DisconnectAsync(true, 
      new CancellationToken(canceled: true));
  }
}
