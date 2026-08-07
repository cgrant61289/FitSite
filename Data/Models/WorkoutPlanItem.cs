namespace FitSite.Data.Models;

public enum WorkoutPlanItemType
{
    Strength = 0,
    Timed = 1
}

public class WorkoutPlanItem
{
    public int Id { get; set; }
    public int WorkoutSessionId { get; set; }
    public string UserId { get; set; } = "";
    public string ExerciseName { get; set; } = "";
    public WorkoutPlanItemType ItemType { get; set; } = WorkoutPlanItemType.Strength;

    public int? Sets { get; set; }
    public int? Reps { get; set; }
    public decimal? WeightLbs { get; set; }

    public int? DurationMinutes { get; set; }
    public decimal? DistanceMiles { get; set; }

    public int DisplayOrder { get; set; }

    public WorkoutSession? WorkoutSession { get; set; }
    public ApplicationUser? User { get; set; }
}