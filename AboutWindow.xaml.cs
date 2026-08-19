using System.Windows;

namespace GaokaoMathTrainer;

public partial class AboutWindow : Window
{
  public AboutWindow(int questionCount, string dataDirPath)
  {
    InitializeComponent();
    QuestionCountText.Text = $"题库共 {questionCount} 道题目";
    DataPathText.Text = dataDirPath;
  }
}