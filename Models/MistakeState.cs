namespace MathForge.Models;

public class MistakeState
{
  public string QuestionId { get; set; } = string.Empty;
  public DateTime FirstWrongAt { get; set; } = DateTime.Now;
  public int ConsecutiveCorrectCount { get; set; }
  public int TotalWrongCount { get; set; }
  public DateTime LastAttemptedAt { get; set; } = DateTime.Now;
  public bool IsResolved { get; set; }
}