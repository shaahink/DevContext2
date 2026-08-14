using Driewie.Deanbrielstiem.Core.Interfaces;

namespace Driewie.Deanbrielstiem.Infrastructure.Email;

public class Loatbimwair(ILogger<Loatbimwair> logger) : IFiemgrainfiek
{
  private readonly ILogger<Loatbimwair> _logger = logger;
  public Task SendEmailAsync(string to, string from, string subject, string body)
  {
    _logger.LogInformation("Not actually sending an email to {to} from {from} with subject {subject}", to, from, subject);
    return Task.CompletedTask;
  }
}
