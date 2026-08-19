using GaokaoMathTrainer.Models;

namespace GaokaoMathTrainer.Services;

public class KnowledgePointSummary
{
  public string Name { get; set; } = string.Empty;
  public int TotalQuestions { get; set; }
}

public static class StatsService
{
  public static List<KnowledgePointSummary> SummarizeByTopLevel(List<Question> questions)
  {
    return questions
        .Where(q => q.KnowledgePoints.Count > 0)
        .GroupBy(q => q.KnowledgePoints[0])
        .Select(g => new KnowledgePointSummary
        {
          Name = g.Key,
          TotalQuestions = g.Count()
        })
        .OrderByDescending(s => s.TotalQuestions)
        .ToList();
  }
}