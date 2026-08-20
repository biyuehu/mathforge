using System.Windows;
using MathForge.Services;

namespace MathForge;

public class ReasonDisplayItem
{
  public string Reason { get; set; } = string.Empty;
  public int Count { get; set; }
  public double BarWidth { get; set; }
}

public partial class AnalysisWindow : Window
{
  public AnalysisWindow(QuestionBank bank, DataStore dataStore)
  {
    InitializeComponent();

    var (total, correct, incorrect, rate) = AnalysisService.Overview(dataStore.Attempts);
    TotalCountText.Text = total.ToString();
    CorrectCountText.Text = correct.ToString();
    IncorrectCountText.Text = incorrect.ToString();
    RateText.Text = total == 0 ? "—" : $"{rate:F0}%";

    var mastery = AnalysisService.MasteryByTopic(bank.Questions, dataStore.Attempts);
    TopicMasteryList.ItemsSource = mastery;

    var reasons = AnalysisService.ReasonDistribution(dataStore.Attempts);
    if (reasons.Count == 0)
    {
      NoReasonHint.Visibility = Visibility.Visible;
    }
    else
    {
      int maxCount = reasons.Max(r => r.Count);
      var displayItems = reasons.Select(r => new ReasonDisplayItem
      {
        Reason = r.Reason,
        Count = r.Count,
        BarWidth = maxCount == 0 ? 2 : Math.Max(2, (double)r.Count / maxCount * 300)
      }).ToList();
      ReasonList.ItemsSource = displayItems;
    }
  }
}