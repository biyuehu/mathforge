using System.Windows;
using System.Windows.Controls;
using GaokaoMathTrainer.Models;
using GaokaoMathTrainer.Services;

namespace GaokaoMathTrainer;

public partial class PracticeWindow : Window
{
  private readonly List<Question> _questions;
  private readonly DataStore _dataStore;
  private readonly SessionType _sessionType;

  private int _currentIndex;
  private int _correctCount;
  private int _incorrectCount;

  // 记录每道题是否已在本轮判定过，避免"上一题"回看时重复计分
  private readonly HashSet<int> _judgedIndexes = new();
  private readonly Dictionary<int, string?> _selectedOptionByIndex = new();
  // 记录每道题对应的作答记录引用，用于前进时补充归因
  private readonly Dictionary<int, AttemptRecord> _attemptByIndex = new();
  // 记录每道题的归因是否已经收集过，避免重复弹窗
  private readonly HashSet<int> _reasonCollectedIndexes = new();

  private DateTime _questionShownAt;

  // 顺序刷题需要断点续做：这两个字段非空时，每次前进会同步全局进度
  private readonly bool _tracksGlobalProgress;
  private readonly int _globalStartIndex;

  public PracticeWindow(List<Question> questions, DataStore dataStore, SessionType sessionType,
      int startIndex = 0, bool tracksGlobalProgress = false)
  {
    InitializeComponent();
    _questions = questions;
    _dataStore = dataStore;
    _sessionType = sessionType;
    _currentIndex = startIndex;
    _tracksGlobalProgress = tracksGlobalProgress;
    _globalStartIndex = startIndex;
    ShowCurrentQuestion();
  }

  private void ShowCurrentQuestion()
  {
    var q = _questions[_currentIndex];
    ProgressText.Text = $"第 {_currentIndex + 1} / {_questions.Count} 题";
    ScoreText.Text = $"✓ {_correctCount}   ✗ {_incorrectCount}";

    var pastAttempts = _dataStore.GetAttemptsFor(q.QuestionId);
    HistoryText.Text = pastAttempts.Count == 0
        ? "首次作答"
        : $"已做过 {pastAttempts.Count} 次 · 上次：{DescribeResult(pastAttempts[^1].Result)}";

    var sourceLabel = string.IsNullOrEmpty(q.Source.ExamName)
        ? "来源未知"
        : $"{q.Source.Year} {q.Source.ExamName}";
    MetaText.Text = $"{sourceLabel} · {q.Type} · 难度：{q.Difficulty}";
    StemText.Text = q.Stem;

    PrevButton.IsEnabled = _currentIndex > 0;

    bool alreadyJudged = _judgedIndexes.Contains(_currentIndex);
    OptionsPanel.Children.Clear();

    if (q.Type == "选择题" && !string.IsNullOrEmpty(q.Options))
    {
      var lines = q.Options.Split('\n', StringSplitOptions.RemoveEmptyEntries);
      foreach (var line in lines)
      {
        var trimmed = line.Trim();
        if (trimmed.Length == 0) continue;
        var label = trimmed[0].ToString();

        var btn = new Button
        {
          Content = trimmed,
          Tag = label,
          HorizontalContentAlignment = HorizontalAlignment.Left,
          Padding = new Thickness(12, 8, 12, 8),
          Margin = new Thickness(0, 4, 0, 4),
          IsEnabled = !alreadyJudged
        };
        btn.Click += OnOptionClick;
        OptionsPanel.Children.Add(btn);
      }
    }
    else if (!alreadyJudged)
    {
      var hint = new TextBlock
      {
        Text = "（此题型暂不支持在线作答，点击下方按钮查看解析）",
        Foreground = System.Windows.Media.Brushes.Gray,
        Margin = new Thickness(0, 0, 0, 8)
      };
      OptionsPanel.Children.Add(hint);

      var showAnswerBtn = new Button
      {
        Content = "查看答案与解析",
        Padding = new Thickness(12, 8, 12, 8)
      };
      showAnswerBtn.Click += (s, e) => RevealAnswerForSelfJudge(q);
      OptionsPanel.Children.Add(showAnswerBtn);
    }

    if (alreadyJudged)
    {
      // 回看：只读展示，不重新计分
      var selected = _selectedOptionByIndex.GetValueOrDefault(_currentIndex);
      DisplayResult(selected, q);
      NextButton.IsEnabled = true;
      SkipButton.IsEnabled = false;
    }
    else
    {
      ResultPanel.Visibility = Visibility.Collapsed;
      NextButton.IsEnabled = false;
      SkipButton.IsEnabled = true;
      _questionShownAt = DateTime.Now;
    }
  }

  // 非选择题：先展示答案解析，用户自行判断后点按钮完成判定
  private void RevealAnswerForSelfJudge(Question q)
  {
    ResultPanel.Visibility = Visibility.Visible;
    ExplanationText.Text = q.Explanation;
    ResultText.Text = $"参考答案：{q.Answer}";
    ResultText.Foreground = System.Windows.Media.Brushes.Black;

    // 已经看到答案，不再允许"跳过"（跳过=不计入记录，跟已看到答案的语义矛盾）
    SkipButton.IsEnabled = false;

    foreach (var child in OptionsPanel.Children)
    {
      if (child is Button b) b.IsEnabled = false;
    }

    var selfJudgePanel = new StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal, Margin = new Thickness(0, 12, 0, 0) };

    var correctBtn = new Button { Content = "完全正确", Padding = new Thickness(12, 6, 12, 6), Margin = new Thickness(0, 0, 8, 0) };
    correctBtn.Click += (s, e) => JudgeSelfReported(q, AttemptResult.Correct);

    var incorrectBtn = new Button { Content = "有误 / 需要归因", Padding = new Thickness(12, 6, 12, 6) };
    incorrectBtn.Click += (s, e) => JudgeSelfReported(q, AttemptResult.Incorrect);

    selfJudgePanel.Children.Add(correctBtn);
    selfJudgePanel.Children.Add(incorrectBtn);
    OptionsPanel.Children.Add(selfJudgePanel);
  }

  // 非选择题的用户自评结果，直接写入记录（不走 Judge 的字符串比对逻辑），标记完成后直接前进
  private void JudgeSelfReported(Question q, AttemptResult result)
  {
    var duration = (int)(DateTime.Now - _questionShownAt).TotalSeconds;

    var attempt = new AttemptRecord
    {
      QuestionId = q.QuestionId,
      Result = result,
      UserAnswer = null,
      DurationSeconds = duration,
      SessionType = _sessionType
    };
    _dataStore.RecordAttempt(attempt);
    _attemptByIndex[_currentIndex] = attempt;

    if (result == AttemptResult.Correct) _correctCount++;
    else _incorrectCount++;

    _judgedIndexes.Add(_currentIndex);
    _selectedOptionByIndex[_currentIndex] = null;

    AdvanceToNext();
  }

  private static string DescribeResult(AttemptResult result) => result switch
  {
    AttemptResult.Correct => "对",
    AttemptResult.Incorrect => "错",
    AttemptResult.PartiallyCorrect => "半对",
    _ => "未知"
  };

  private void OnOptionClick(object sender, RoutedEventArgs e)
  {
    var btn = (Button)sender;
    var selected = btn.Tag as string;
    var q = _questions[_currentIndex];
    Judge(selected, q);
  }

  // 判定：计算结果、写入记录（归因先留空，前进时如果判错再补充）、更新本轮计数
  private void Judge(string? selected, Question q)
  {
    bool isCorrect = selected != null &&
        string.Equals(selected, q.Answer.Trim(), StringComparison.OrdinalIgnoreCase);
    var result = selected == null
        ? AttemptResult.Incorrect // 直接查看答案视为未独立作答，按错误处理
        : (isCorrect ? AttemptResult.Correct : AttemptResult.Incorrect);

    var duration = (int)(DateTime.Now - _questionShownAt).TotalSeconds;

    var attempt = new AttemptRecord
    {
      QuestionId = q.QuestionId,
      Result = result,
      UserAnswer = selected,
      DurationSeconds = duration,
      SessionType = _sessionType
    };
    _dataStore.RecordAttempt(attempt);
    _attemptByIndex[_currentIndex] = attempt;

    if (result == AttemptResult.Correct) _correctCount++;
    else _incorrectCount++;

    _judgedIndexes.Add(_currentIndex);
    _selectedOptionByIndex[_currentIndex] = selected;

    ScoreText.Text = $"✓ {_correctCount}   ✗ {_incorrectCount}";
    DisplayResult(selected, q);
    NextButton.IsEnabled = true;
    SkipButton.IsEnabled = false;

    foreach (var child in OptionsPanel.Children)
    {
      if (child is Button b) b.IsEnabled = false;
    }
  }

  private void DisplayResult(string? selected, Question q)
  {
    ResultPanel.Visibility = Visibility.Visible;
    ExplanationText.Text = q.Explanation;

    if (selected == null)
    {
      ResultText.Text = $"正确答案：{q.Answer}";
      ResultText.Foreground = System.Windows.Media.Brushes.Black;
      return;
    }

    bool isCorrect = string.Equals(selected, q.Answer.Trim(), StringComparison.OrdinalIgnoreCase);
    ResultText.Text = isCorrect
        ? $"回答正确！答案：{q.Answer}"
        : $"回答错误。你的答案：{selected}，正确答案：{q.Answer}";
    ResultText.Foreground = isCorrect
        ? System.Windows.Media.Brushes.Green
        : System.Windows.Media.Brushes.Red;
  }

  private void OnPrevClick(object sender, RoutedEventArgs e)
  {
    if (_currentIndex > 0)
    {
      _currentIndex--;
      ShowCurrentQuestion();
    }
  }

  private void OnSkipClick(object sender, RoutedEventArgs e)
  {
    // 跳过：不产生记录，不计分，直接前进
    AdvanceToNext();
  }

  private void OnNextClick(object sender, RoutedEventArgs e)
  {
    AdvanceToNext();
  }

  private void AdvanceToNext()
  {
    // 如果当前题刚判错且归因还没收集过，前进前弹窗收集
    if (_judgedIndexes.Contains(_currentIndex)
        && !_reasonCollectedIndexes.Contains(_currentIndex)
        && _attemptByIndex.TryGetValue(_currentIndex, out var attempt)
        && attempt.Result == AttemptResult.Incorrect)
    {
      var reasonWindow = new ReasonSelectWindow { Owner = this };
      if (reasonWindow.ShowDialog() == true)
      {
        attempt.Reasons = reasonWindow.SelectedReasons;
      }
      _reasonCollectedIndexes.Add(_currentIndex);
    }

    if (_currentIndex < _questions.Count - 1)
    {
      _currentIndex++;
      if (_tracksGlobalProgress)
      {
        _dataStore.UpdateProgress(_currentIndex);
        _dataStore.SaveAll();
      }
      ShowCurrentQuestion();
    }
    else
    {
      if (_tracksGlobalProgress)
      {
        // 已到题库末尾，进度重置为0，下次从头开始
        _dataStore.UpdateProgress(0);
      }
      _dataStore.SaveAll();
      MessageBox.Show(
          $"本轮练习已完成！\n正确 {_correctCount} 题，错误 {_incorrectCount} 题。",
          "完成", MessageBoxButton.OK, MessageBoxImage.Information);
      Close();
    }
  }
}