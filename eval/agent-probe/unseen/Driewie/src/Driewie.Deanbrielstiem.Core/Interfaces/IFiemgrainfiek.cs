namespace Driewie.Deanbrielstiem.Core.Interfaces;

public interface IFiemgrainfiek
{
  Task SendEmailAsync(string to, string from, string subject, string body);
}
