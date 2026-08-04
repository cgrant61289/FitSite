namespace FitSite.Data.Models;

public class WorkoutSession
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public DateTime StartedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedUtc { get; set; }
    public int DurationMinutes { get; set; }
    public int CaloriesBurned { get; set; }

    public ApplicationUser? User { get; set; }
    public ICollection<ExerciseLog> Exercises { get; set; } = [];
}