using Microsoft.AspNetCore.Identity.UI.Services;

namespace DSJsBookStore.Shared;

public class AppEmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        return Task.CompletedTask;
    }
}
