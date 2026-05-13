namespace MechanicShop.Infrastructure.Settings;

public class AppSettings
{
    public TimeOnly OpeningTime {get; set; }
    public TimeOnly ClosingTime {get; set; }
    public int MaxSpots {get; set; }
    public int MinAppointmentDurationInMinutes {get; set; }
    public int LocalCacheExpirationInMinutes {get; set; }
    public int DistributedCacheExpirationInMinutes {get; set; }
    public int DefaultPageNumber {get; set; }
    public int DefaultPageSize {get; set; }
    public int CleanupJobIntervalInMinutes {get; set; }
    public int CancellationDeadlineInMinutes {get; set; }
    public string CorsPolicyName {get; set; } = default!;
    public string[] AllowedOrigins {get; set;} = default!;
}