namespace GaokaoMathTrainer.Models;

public enum AttemptResult
{
  Correct,
  Incorrect,
  PartiallyCorrect
}

public enum AttemptReason
{
  KnowledgeGap,
  ConceptConfusion,
  StuckOnApproach,
  MisreadQuestion,
  CalculationError,
  ForgotUnitOrFormat,
  Guessed,
  ChainFailureFromEarlierPart
}

public enum SessionType
{
  SequentialPractice,
  FilteredPractice,
  CategoryPractice,
  MistakePractice
}

public class AttemptRecord
{
  public string QuestionId { get; set; } = string.Empty;
  public DateTime AttemptedAt { get; set; } = DateTime.Now;
  public AttemptResult Result { get; set; }
  public List<AttemptReason> Reasons { get; set; } = new();
  public string? UserAnswer { get; set; }
  public int? DurationSeconds { get; set; }
  public SessionType SessionType { get; set; }
}