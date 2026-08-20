using System.Diagnostics;
using System.Windows;

namespace MathForge;

public partial class AboutWindow : Window
{
  public AboutWindow(int questionCount, string dataDirPath)
  {
    InitializeComponent();
    QuestionCountText.Text = $"题库共 {questionCount} 道题目";
    DataPathText.Text = dataDirPath;
  }

  private void ProjectLink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
  {
    Process.Start(new ProcessStartInfo
    {
      FileName = e.Uri.AbsoluteUri,
      UseShellExecute = true
    });
    e.Handled = true;
  }
}