using System.Windows;
using GaokaoMathTrainer.Services;

namespace GaokaoMathTrainer;

public partial class AnalysisWindow : Window
{
  public AnalysisWindow(QuestionBank bank, DataStore dataStore)
  {
    InitializeComponent();

    var (total, correct, incorrect, rate) = AnalysisService.Overview(dataStore.Attempts);
    OverviewText.Text = total == 0
        ? "暂无作答记录，快去刷题吧！"
        : $"共作答 {total} 次，正确 {correct} 次，错误 {incorrect} 次，正确率 {rate:F1}%";

    var mastery = AnalysisService.MasteryByTopic(bank.Questions, dataStore.Attempts);
    TopicMasteryList.ItemsSource = mastery;

    var reasons = AnalysisService.ReasonDistribution(dataStore.Attempts);
    ReasonList.ItemsSource = reasons;
  }
}