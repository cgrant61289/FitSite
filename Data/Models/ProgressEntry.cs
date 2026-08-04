namespace FitSite.Data.Models;

public class ProgressEntry
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public DateTime RecordedUtc { get; set; } = DateTime.UtcNow;
    public decimal? WeightLbs { get; set; }
    public int? BodyFatPercent { get; set; }
    public string? Notes { get; set; }

    public ApplicationUser? User { get; set; }
}