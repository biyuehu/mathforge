using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using GaokaoMathTrainer.Models;

namespace GaokaoMathTrainer.Services;

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
  public static QuestionBank Load(string jsonPath)
  {
    var json = File.ReadAllText(jsonPath);
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