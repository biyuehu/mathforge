using System.Windows;
using System.Windows.Controls;
using GaokaoMathTrainer.Models;
using GaokaoMathTrainer.Services;

namespace GaokaoMathTrainer;

public class MistakeListItem
{
  public string StemPreview { get; set; } = string.Empty;
  public string StatusLabel { get; set; } = string.Empty;
}

public partial class MistakeWindow : Window
{
  private readonly QuestionBank _bank;
  private readonly DataStore _dataStore;
  private List<Question> _mistakeQuestions = new();

  public MistakeWindow(QuestionBank bank, DataStore dataStore)
  {
    _bank = bank;
    _dataStore = dataStore;
    InitializeComponent();
    ThresholdCombo.SelectedIndex = 0; // 在 InitializeComponent 完成后设置，触发 SelectionChanged 时所有控件已就绪
  }

  private int SelectedThreshold => ThresholdCombo.SelectedIndex + 1;

  private void OnThresholdChanged(object sender, SelectionChangedEventArgs e) => RefreshList();

  private void RefreshList()
  {
    _dataStore.ApplyMistakeResolutionThreshold(SelectedThreshold);

    var unresolvedIds = _dataStore.Mistakes.Values
        .Where(m => !m.IsResolved)
        .Select(m => m.QuestionId)
        .ToHashSet();

    _mistakeQuestions = _bank.Questions.Where(q => unresolvedIds.Contains(q.QuestionId)).ToList();

    var displayItems = _mistakeQuestions.Select(q =>
    {
      var state = _dataStore.Mistakes[q.QuestionId];
      var preview = q.Stem.Length > 60 ? q.Stem[..60] + "…" : q.Stem;
      return new MistakeListItem
      {
        StemPreview = preview,
        StatusLabel = $"累计错误 {state.TotalWrongCount} 次 · 当前连对 {state.ConsecutiveCorrectCount}/{SelectedThreshold}"
      };
    }).ToList();

    MistakeList.ItemsSource = displayItems;
    MistakeCountText.Text = $"共 {_mistakeQuestions.Count} 道待解决错题";
  }

  private void OnStartClick(object sender, RoutedEventArgs e)
  {
    if (_mistakeQuestions.Count == 0)
    {
      MessageBox.Show("当前没有待解决的错题。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
      return;
    }

    var practiceWindow = new PracticeWindow(_mistakeQuestions, _dataStore, SessionType.MistakePractice)
    {
      Owner = this
    };
    Close();
    practiceWindow.ShowDialog();
  }
}