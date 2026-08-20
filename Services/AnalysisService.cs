using MathForge.Models;

namespace MathForge.Services;

public class TopicMastery
{
  public string Topic { get; set; } = string.Empty;
  public int Attempted { get; set; }
  public double CorrectRate { get; set; }
}

public class ReasonStat
{
  public string Reason { get; set; } = string.Empty;
  public int Count { get; set; }
}

public static class AnalysisService
{
  public static (int total, int correct, int incorrect, double correctRate) Overview(List<AttemptRecord> attempts)
  {
    int total = attempts.Count;
    int correct = attempts.Count(a => a.Result == AttemptResult.Correct);
    int incorrect = attempts.Count(a => a.Result == AttemptResult.Incorrect);
    double rate = total == 0 ? 0 : (double)correct / total * 100;
    return (total, correct, incorrect, rate);
  }

  // 按知识板块第一级聚合掌握度：正确率 = 该板块下所有作答记录中"正确"占比
  public static List<TopicMastery> MasteryByTopic(List<Question> questions, List<AttemptRecord> attempts)
  {
    var questionTopicMap = questions
        .Where(q => q.KnowledgePoints.Count > 0)
        .GroupBy(q => q.QuestionId)
        .ToDictionary(g => g.Key, g => g.First().KnowledgePoints[0]);

    var grouped = attempts
        .Where(a => questionTopicMap.ContainsKey(a.QuestionId))
        .GroupBy(a => questionTopicMap[a.QuestionId]);

    return grouped.Select(g =>
    {
      var list = g.ToList();
      int correctCount = list.Count(a => a.Result == AttemptResult.Correct);
      return new TopicMastery
      {
        Topic = g.Key,
        Attempted = list.Count,
        CorrectRate = list.Count == 0 ? 0 : (double)correctCount / list.Count * 100
      };
    })
    .OrderBy(t => t.CorrectRate)
    .ToList();
  }

  // 归因分布：只统计"错误"结果的记录
  public static List<ReasonStat> ReasonDistribution(List<AttemptRecord> attempts)
  {
    return attempts
        .Where(a => a.Result == AttemptResult.Incorrect)
        .SelectMany(a => a.Reasons)
        .GroupBy(r => r)
        .Select(g => new ReasonStat { Reason = ReasonLabels.Describe(g.Key), Count = g.Count() })
        .OrderByDescending(s => s.Count)
        .ToList();
  }
}