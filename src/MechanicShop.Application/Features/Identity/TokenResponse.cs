namespace MechanicShop.Application.Features.Identity;
public class TokenResponse
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
}