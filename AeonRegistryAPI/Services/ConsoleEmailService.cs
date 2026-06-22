using Microsoft.AspNetCore.Identity.UI.Services; 

namespace AeonRegistryAPI.Services
{
    public class ConsoleEmailService : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            Console.WriteLine($"To:{email}\nSubject:{subject}\n\n{htmlMessage}");
            return Task.CompletedTask;
        }
    }
}
