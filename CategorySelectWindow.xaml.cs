using System.Windows;
using System.Windows.Controls;
using GaokaoMathTrainer.Models;
using GaokaoMathTrainer.Services;

namespace GaokaoMathTrainer;

public partial class CategorySelectWindow : Window
{
  private readonly QuestionBank _bank;
  private readonly DataStore _dataStore;

  public CategorySelectWindow(QuestionBank bank, DataStore dataStore)
  {
    InitializeComponent();
    _bank = bank;
    _dataStore = dataStore;

    var summaries = StatsService.SummarizeByTopLevel(_bank.Questions);
    TopicList.ItemsSource = summaries;
  }

  private void OnTopicCardClick(object sender, RoutedEventArgs e)
  {
    var button = (Button)sender;
    var topicName = button.Tag as string;
    if (string.IsNullOrEmpty(topicName)) return;

    var questions = _bank.Questions
        .Where(q => q.KnowledgePoints.Count > 0 && q.KnowledgePoints[0] == topicName)
        .ToList();

    if (questions.Count == 0)
    {
      MessageBox.Show("该知识板块下暂无题目。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
      return;
    }

    var practiceWindow = new PracticeWindow(questions, _dataStore, SessionType.CategoryPractice)
    {
      Owner = this
    };
    practiceWindow.ShowDialog();
  }
}