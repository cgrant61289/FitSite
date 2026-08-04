namespace FitSite.Data.Models;

public class ExerciseLog
{
    public int Id { get; set; }
    public int WorkoutSessionId { get; set; }
    public string UserId { get; set; } = "";
    public string ExerciseName { get; set; } = "";
    public int Sets { get; set; }
    public int Reps { get; set; }
    public decimal WeightLbs { get; set; }
    public DateTime LoggedUtc { get; set; } = DateTime.UtcNow;

    public WorkoutSession? WorkoutSession { get; set; }
    public ApplicationUser? User { get; set; }
}