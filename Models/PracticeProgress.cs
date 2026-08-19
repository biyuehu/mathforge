namespace GaokaoMathTrainer.Models;

public class PracticeProgress
{
  public int LastQuestionIndex { get; set; }
  public DateTime LastUpdatedAt { get; set; } = DateTime.Now;
}