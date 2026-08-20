using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MathForge.Models;

namespace MathForge.Services;

public class QuestionBank
{
  public MetaInfo Meta { get; set; } = new();
  public List<Question> Questions { get; set; } = new();
}

public class MetaInfo
{
  public string Source { get; set; } = string.Empty;
  public int TotalCount { get; set; }
}

public static class QuestionLoader
{
  // 从嵌入资源加载题库，不再依赖磁盘文件路径
  public static QuestionBank LoadFromEmbeddedResource()
  {
    var assembly = Assembly.GetExecutingAssembly();
    var resourceName = "MathForge.Assets.gaokao-math.json";
    using var stream = assembly.GetManifestResourceStream(resourceName);
    if (stream == null)
    {
      throw new InvalidOperationException(
          $"未找到嵌入资源: {resourceName}。可用资源: {string.Join(", ", assembly.GetManifestResourceNames())}");
    }
    using var reader = new StreamReader(stream);
    var json = reader.ReadToEnd();
    return LoadFromJson(json);
  }

  public static QuestionBank Load(string jsonPath)
  {
    var json = File.ReadAllText(jsonPath);
    return LoadFromJson(json);
  }

  private static QuestionBank LoadFromJson(string json)
  {
    var options = new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    };
    var bank = JsonSerializer.Deserialize<QuestionBank>(json, options)
        ?? throw new InvalidDataException("题库解析失败");
    foreach (var q in bank.Questions)
    {
      q.QuestionId = GenerateStableId(q);
    }
    return bank;
  }

  // 基于 Year+ExamName+Type+Stem 生成稳定哈希ID，避免依赖数组下标
  private static string GenerateStableId(Question q)
  {
    var raw = $"{q.Source.Year}|{q.Source.ExamName}|{q.Type}|{q.Stem}";
    var bytes = Encoding.UTF8.GetBytes(raw);
    var hash = SHA256.HashData(bytes);
    return Convert.ToHexString(hash)[..16];
  }
}