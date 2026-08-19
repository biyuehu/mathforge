using System.Windows;
using System.Windows.Controls;
using GaokaoMathTrainer.Models;

namespace GaokaoMathTrainer;

public partial class ReasonSelectWindow : Window
{
  private static readonly Dictionary<AttemptReason, string> ReasonLabels = new()
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

  private readonly Dictionary<AttemptReason, CheckBox> _checkBoxes = new();

  public List<AttemptReason> SelectedReasons { get; private set; } = new();

  public ReasonSelectWindow()
  {
    InitializeComponent();
    foreach (var (reason, label) in ReasonLabels)
    {
      var cb = new CheckBox { Content = label, Margin = new Thickness(0, 0, 0, 10), Tag = reason };
      _checkBoxes[reason] = cb;
      ReasonPanel.Children.Add(cb);
    }
  }

  private void OnConfirmClick(object sender, RoutedEventArgs e)
  {
    SelectedReasons = _checkBoxes
        .Where(kv => kv.Value.IsChecked == true)
        .Select(kv => kv.Key)
        .ToList();
    DialogResult = true;
  }

  private void OnSkipClick(object sender, RoutedEventArgs e)
  {
    SelectedReasons = new List<AttemptReason>();
    DialogResult = true;
  }
}