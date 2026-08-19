using System.Windows;
using System.Windows.Controls;
using GaokaoMathTrainer.Models;
using GaokaoMathTrainer.Services;

namespace GaokaoMathTrainer;

public partial class FilterWindow : Window
{
  private readonly QuestionBank _bank;
  private readonly DataStore _dataStore;
  private List<Question> _matched = new();

  public FilterWindow(QuestionBank bank, DataStore dataStore)
  {
    InitializeComponent();
    _bank = bank;
    _dataStore = dataStore;

    var topics = StatsService.SummarizeByTopLevel(_bank.Questions);
    TopicCheckList.ItemsSource = topics;

    TypeCheckList.ItemsSource = _bank.Questions.Select(q => q.Type).Distinct().OrderBy(t => t).ToList();
    DifficultyCheckList.ItemsSource = _bank.Questions.Select(q => q.Difficulty).Distinct().OrderBy(d => d).ToList();

    RecomputeMatches();
  }

  private void OnFilterChanged(object sender, RoutedEventArgs e) => RecomputeMatches();

  private void OnCountInputChanged(object sender, TextChangedEventArgs e) => ValidateCountInput();

  private List<string> GetCheckedTags(ItemsControl list)
  {
    var result = new List<string>();
    foreach (var item in list.Items)
    {
      var container = list.ItemContainerGenerator.ContainerFromItem(item) as ContentPresenter;
      if (container == null) continue;
      var checkBox = FindCheckBox(container);
      if (checkBox != null && checkBox.IsChecked == true && checkBox.Tag is string tag)
      {
        result.Add(tag);
      }
    }
    return result;
  }

  private static CheckBox? FindCheckBox(DependencyObject parent)
  {
    for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
    {
      var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
      if (child is CheckBox cb) return cb;
      var found = FindCheckBox(child);
      if (found != null) return found;
    }
    return null;
  }

  private void RecomputeMatches()
  {
    var selectedTopics = GetCheckedTags(TopicCheckList);
    var selectedTypes = GetCheckedTags(TypeCheckList);
    var selectedDifficulties = GetCheckedTags(DifficultyCheckList);
    bool onlyUnattempted = OnlyUnattemptedCheck.IsChecked == true;

    var attemptedIds = onlyUnattempted
      ? _dataStore.Attempts.Select(a => a.QuestionId).ToHashSet()
      : null;

    _matched = _bank.Questions.Where(q =>
    {
      if (selectedTopics.Count > 0 && (q.KnowledgePoints.Count == 0 || !selectedTopics.Contains(q.KnowledgePoints[0])))
        return false;
      if (selectedTypes.Count > 0 && !selectedTypes.Contains(q.Type))
        return false;
      if (selectedDifficulties.Count > 0 && !selectedDifficulties.Contains(q.Difficulty))
        return false;
      if (attemptedIds != null && attemptedIds.Contains(q.QuestionId))
        return false;
      return true;
    }).ToList();

    HitCountText.Text = $"当前筛选共命中 {_matched.Count} 题";
    ValidateCountInput();
  }

  private void ValidateCountInput()
  {
    if (_matched.Count == 0)
    {
      HitCountText.Text = "当前筛选共命中 0 题，请调整筛选条件";
      return;
    }

    if (int.TryParse(CountInput.Text, out int requested) && requested > 0)
    {
      if (requested > _matched.Count)
      {
        HitCountText.Text = $"当前筛选共命中 {_matched.Count} 题，超出可用题目数，将按 {_matched.Count} 题处理";
      }
      else
      {
        HitCountText.Text = $"当前筛选共命中 {_matched.Count} 题，将练习 {requested} 题";
      }
    }
    else
    {
      HitCountText.Text = $"当前筛选共命中 {_matched.Count} 题";
    }
  }

  private void OnStartClick(object sender, RoutedEventArgs e)
  {
    if (_matched.Count == 0)
    {
      MessageBox.Show("当前筛选条件下没有匹配的题目，请调整筛选条件。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
      return;
    }

    int count = _matched.Count;
    if (int.TryParse(CountInput.Text, out int requested) && requested > 0)
    {
      count = Math.Min(requested, _matched.Count);
    }

    // 默认随机打乱
    var shuffled = _matched.OrderBy(_ => Guid.NewGuid()).Take(count).ToList();

    var owner = Owner;
    var practiceWindow = new PracticeWindow(shuffled, _dataStore, SessionType.FilteredPractice)
    {
      Owner = owner
    };
    practiceWindow.ShowDialog();
    Close();
  }
}