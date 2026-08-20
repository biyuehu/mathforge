using System.Windows;
using MathForge.Models;
using MathForge.Services;

namespace MathForge;

public partial class MainWindow : Window
{
  private QuestionBank? _bank;
  private DataStore? _dataStore;

  public MainWindow()
  {
    InitializeComponent();
    LoadBank();
    Closing += OnWindowClosing;
  }

  private void LoadBank()
  {
    try
    {
      _bank = QuestionLoader.LoadFromEmbeddedResource();
      _dataStore = new DataStore();
      StatusText.Text = $"已加载 {_bank.Questions.Count} 道题目 · 已完成 {_dataStore.Attempts.Count} 次作答";
    }
    catch (Exception ex)
    {
      MessageBox.Show($"加载题库失败:\n{ex}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
    }
  }

  private void OnWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
  {
    _dataStore?.SaveAll();
  }

  private bool EnsureReady()
  {
    if (_bank == null || _dataStore == null)
    {
      MessageBox.Show("题库尚未加载完成。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
      return false;
    }
    return true;
  }

  private void OnSequentialClick(object sender, RoutedEventArgs e)
  {
    if (!EnsureReady()) return;

    var startIndex = _dataStore!.Progress.LastQuestionIndex;
    if (startIndex >= _bank!.Questions.Count) startIndex = 0;

    var practiceWindow = new PracticeWindow(
        _bank.Questions, _dataStore, SessionType.SequentialPractice,
        startIndex: startIndex, tracksGlobalProgress: true)
    {
      Owner = this
    };
    practiceWindow.ShowDialog();

    StatusText.Text = $"已加载 {_bank.Questions.Count} 道题目 · 已完成 {_dataStore.Attempts.Count} 次作答";
  }

  private void OnFilteredClick(object sender, RoutedEventArgs e)
  {
    if (!EnsureReady()) return;
    var window = new FilterWindow(_bank!, _dataStore!)
    {
      Owner = this
    };
    window.ShowDialog();
  }

  private void OnCategoryClick(object sender, RoutedEventArgs e)
  {
    if (!EnsureReady()) return;
    var window = new CategorySelectWindow(_bank!, _dataStore!)
    {
      Owner = this
    };
    window.ShowDialog();
  }

  private void OnMistakeClick(object sender, RoutedEventArgs e)
  {
    if (!EnsureReady()) return;
    var window = new MistakeWindow(_bank!, _dataStore!)
    {
      Owner = this
    };
    window.ShowDialog();
  }

  private void OnAnalysisClick(object sender, RoutedEventArgs e)
  {
    if (!EnsureReady()) return;
    var window = new AnalysisWindow(_bank!, _dataStore!)
    {
      Owner = this
    };
    window.ShowDialog();
  }

  private void OnAboutClick(object sender, RoutedEventArgs e)
  {
    if (!EnsureReady()) return;
    var about = new AboutWindow(_bank!.Questions.Count, _dataStore!.DataDirPath)
    {
      Owner = this
    };
    about.ShowDialog();
  }
}