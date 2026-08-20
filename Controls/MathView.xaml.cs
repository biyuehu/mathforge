using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Controls;

namespace MathForge.Controls;

public partial class MathView : UserControl
{
  private const string VirtualHostName = "appassets.local";
  private static string? _extractedFolder;

  private bool _isReady;
  private string _pendingHtmlBody = string.Empty;
  private bool _hasPending;

  public event Action<string>? OptionClicked;

  public MathView()
  {
    InitializeComponent();
    Loaded += async (s, e) => await EnsureInitialized();
  }

  private async System.Threading.Tasks.Task EnsureInitialized()
  {
    if (_isReady) return;
    EnsureAssetsExtracted();
    await Browser.EnsureCoreWebView2Async();
    Browser.CoreWebView2.SetVirtualHostNameToFolderMapping(
        VirtualHostName, _extractedFolder!, Microsoft.Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow);
    Browser.CoreWebView2.WebMessageReceived += OnWebMessageReceived;

    _isReady = true;
    if (_hasPending)
    {
      NavigateWithBody(_pendingHtmlBody);
      _hasPending = false;
    }
  }

  private void OnWebMessageReceived(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
  {
    var label = e.TryGetWebMessageAsString();
    if (!string.IsNullOrEmpty(label))
    {
      OptionClicked?.Invoke(label);
    }
  }

  // 把嵌入资源里的 MathJax 脚本释放到临时目录，只在应用生命周期内首次调用时执行一次
  private static void EnsureAssetsExtracted()
  {
    if (_extractedFolder != null) return;
    var folder = Path.Combine(Path.GetTempPath(), "MathForgeAssets");
    Directory.CreateDirectory(folder);
    var jsPath = Path.Combine(folder, "tex-svg.js");
    if (!File.Exists(jsPath))
    {
      var assembly = Assembly.GetExecutingAssembly();
      var resourceName = "MathForge.Assets.tex-svg.js";
      using var stream = assembly.GetManifestResourceStream(resourceName);
      if (stream == null)
      {
        throw new InvalidOperationException(
            $"未找到嵌入资源: {resourceName}。可用资源: {string.Join(", ", assembly.GetManifestResourceNames())}");
      }
      using var fileStream = File.Create(jsPath);
      stream.CopyTo(fileStream);
    }
    _extractedFolder = folder;
  }

  // 传入纯文本，内含 $...$ / $...$ 形式的 LaTeX 片段，会用 MathJax 渲染
  // color 可选，传入 CSS 颜色值（如 "green"、"#CC0000"）控制文字颜色
  public void SetContent(string text, string? color = null)
  {
    var escaped = System.Net.WebUtility.HtmlEncode(text).Replace("\n", "<br/>");
    var styledColor = string.IsNullOrEmpty(color) ? "" : $" style=\"color:{color}; font-weight:bold;\"";
    var html = $"<div{styledColor}>{escaped}</div>";
    SetRawHtml(html);
  }

  // 渲染一组可点击的选项，label 用于标识（如 "A"），点击后触发 OptionClicked
  public void SetOptions(List<(string label, string text)> options)
  {
    var sb = new StringBuilder();
    foreach (var (label, text) in options)
    {
      var escaped = System.Net.WebUtility.HtmlEncode(text);
      sb.Append($"<div class='option' onclick=\"window.chrome.webview.postMessage('{label}')\">{escaped}</div>");
    }
    SetRawHtml(sb.ToString());
  }

  private void SetRawHtml(string html)
  {
    if (!_isReady)
    {
      _pendingHtmlBody = html;
      _hasPending = true;
      return;
    }
    NavigateWithBody(html);
  }

  private void NavigateWithBody(string bodyHtml)
  {
    var html = BuildHtml(bodyHtml);
    Browser.NavigateToString(html);
  }

  private static string BuildHtml(string bodyHtml)
  {
    var sb = new StringBuilder();
    sb.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\"/>");
    sb.Append("<script>");
    sb.Append("MathJax = { tex: { inlineMath: [['$', '$']], displayMath: [['$$', '$$']] }, svg: { fontCache: 'global' } };");
    sb.Append("</script>");
    sb.Append($"<script src=\"https://{VirtualHostName}/tex-svg.js\"></script>");
    sb.Append("<style>");
    sb.Append("body { font-family: \"Microsoft YaHei\", sans-serif; font-size: 16px; margin: 0; padding: 4px; line-height: 1.6; }");
    sb.Append(".option { padding: 10px 12px; margin: 4px 0; border: 1px solid #DDDDDD; border-radius: 6px; cursor: pointer; }");
    sb.Append(".option:hover { background: #F5F5F5; }");
    sb.Append(".option.disabled { pointer-events: none; opacity: 0.6; }");
    sb.Append("</style>");
    sb.Append("</head><body>");
    sb.Append(bodyHtml);
    sb.Append("</body></html>");
    return sb.ToString();
  }
}