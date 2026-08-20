using System.Windows;
using System.Windows.Threading;

namespace MathForge;

public partial class App : Application
{
  protected override void OnStartup(StartupEventArgs e)
  {
    base.OnStartup(e);
    DispatcherUnhandledException += OnDispatcherUnhandledException;
    AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
  }

  private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
  {
    MessageBox.Show(
        $"未处理异常:\n{e.Exception}",
        "启动错误",
        MessageBoxButton.OK,
        MessageBoxImage.Error);
    e.Handled = true;
  }

  private void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
  {
    MessageBox.Show(
        $"致命异常:\n{e.ExceptionObject}",
        "启动错误",
        MessageBoxButton.OK,
        MessageBoxImage.Error);
  }
}
