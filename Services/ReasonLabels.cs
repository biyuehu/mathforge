using MathForge.Models;

namespace MathForge.Services;

public static class ReasonLabels
{
  public static readonly Dictionary<AttemptReason, string> Map = new()
  {
    [AttemptReason.KnowledgeGap] = "知识点不会/没学过",
    [AttemptReason.ConceptConfusion] = "概念混淆",
    [AttemptReason.StuckOnApproach] = "思路卡住",
    [AttemptReason.MisreadQuestion] = "看错题目/条件",
    [AttemptReason.CalculationError] = "计算失误",
    [AttemptReason.ForgotUnitOrFormat] = "忘记单位/格式要求",
    [AttemptReason.Guessed] = "蒙对/蒙错",
    [AttemptReason.ChainFailureFromEarlierPart] = "某一问卡住导致连锁",
  };

  public static string Describe(AttemptReason reason) =>
      Map.TryGetValue(reason, out var label) ? label : reason.ToString();
}