using Microsoft.AspNetCore.Identity;

namespace FitSite.Data.Models;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public ICollection<WorkoutSession> WorkoutSessions { get; set; } = new List<WorkoutSession>();
    public ICollection<ExerciseLog> ExerciseLogs { get; set; } = new List<ExerciseLog>();
}