using MechanicShop.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Infrastructure.Services;

public sealed class NotificationService(
    ILogger<NotificationService> logger
) : INotificationService
{
    private readonly ILogger<NotificationService> _logger = logger;
    private const string Message = "Your vehicle service is complete. You may collect it from the shop at your earliest convenience.";
    public async Task SendEmailAsync(string to, CancellationToken cancellationToken = default)
    {
        var atIndex = to.IndexOf('@');

        var maskedEmail = atIndex > 0 
            ? to[0] + new string('*', atIndex - 2) + to[atIndex - 1] + to[^atIndex..] 
            : "*****" ;

        _logger.LogInformation("[Email] To: {Email} | Message: {Message}", maskedEmail, Message);
        await Task.CompletedTask;    
    }

    public async Task SendSmsAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        var maskedPhoneNumber = phoneNumber.Length >= 4 
            ? new string('*', phoneNumber.Length - 4) + phoneNumber[^4..]
            : "*****";
        _logger.LogInformation("[SMS] To: {Phone} | Message: {Message}", maskedPhoneNumber, Message);

        await Task.CompletedTask;
    }
}