namespace LiftAndShift.Application.Common.Models;

public class UserOnboardingDto
{
    public bool IsOnboarded { get; set; }
    public string PreferredUnit { get; set; } = "Lbs";
    public decimal? BodyWeight { get; set; }
    public string AlternatingLift { get; set; } = "PowerClean";
    public decimal? SquatStartingWeight { get; set; }
    public decimal? BenchPressStartingWeight { get; set; }
    public decimal? OverheadPressStartingWeight { get; set; }
    public decimal? DeadliftStartingWeight { get; set; }
    public decimal? AlternatingLiftStartingWeight { get; set; }
}
