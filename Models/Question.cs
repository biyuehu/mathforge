namespace MathForge.Models;

public class Question
{
  [System.Text.Json.Serialization.JsonIgnore]
  public string QuestionId { get; set; } = string.Empty;

  public Source Source { get; set; } = new();
  public string Type { get; set; } = string.Empty;
  public string? Number { get; set; }
  public string Stem { get; set; } = string.Empty;
  public string? Options { get; set; }
  public string Answer { get; set; } = string.Empty;
  public string Explanation { get; set; } = string.Empty;
  public List<string> KnowledgePoints { get; set; } = new();
  public string Difficulty { get; set; } = string.Empty;
  public List<string> Keywords { get; set; } = new();
}