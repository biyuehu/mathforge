using System.IO;
using System.Text.Json;
using MathForge.Models;

namespace MathForge.Services;

public class DataStore
{
  private readonly string _dataDir;
  private readonly string _attemptsPath;
  private readonly string _mistakesPath;
  private readonly string _progressPath;

  private readonly JsonSerializerOptions _jsonOptions = new()
  {
    WriteIndented = true,
    Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
  };

  public List<AttemptRecord> Attempts { get; private set; } = new();
  public Dictionary<string, MistakeState> Mistakes { get; private set; } = new();
  public PracticeProgress Progress { get; private set; } = new();
  public string DataDirPath => _dataDir;

  public DataStore()
  {
    _dataDir = Path.Combine(
      Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
      "MathForge");
    Directory.CreateDirectory(_dataDir);

    _attemptsPath = Path.Combine(_dataDir, "attempts.json");
    _mistakesPath = Path.Combine(_dataDir, "mistakes.json");
    _progressPath = Path.Combine(_dataDir, "progress.json");

    LoadAll();
  }

  private void LoadAll()
  {
    Attempts = LoadJson<List<AttemptRecord>>(_attemptsPath) ?? new List<AttemptRecord>();
    var mistakeList = LoadJson<List<MistakeState>>(_mistakesPath) ?? new List<MistakeState>();
    Mistakes = mistakeList.ToDictionary(m => m.QuestionId);
    Progress = LoadJson<PracticeProgress>(_progressPath) ?? new PracticeProgress();
  }

  private T? LoadJson<T>(string path) where T : class
  {
    if (!File.Exists(path)) return null;
    var json = File.ReadAllText(path);
    if (string.IsNullOrWhiteSpace(json)) return null;
    return JsonSerializer.Deserialize<T>(json, _jsonOptions);
  }

  public void SaveAll()
  {
    File.WriteAllText(_attemptsPath, JsonSerializer.Serialize(Attempts, _jsonOptions));
    File.WriteAllText(_mistakesPath, JsonSerializer.Serialize(Mistakes.Values.ToList(), _jsonOptions));
    File.WriteAllText(_progressPath, JsonSerializer.Serialize(Progress, _jsonOptions));
  }

  // 记录一次作答，并同步更新错题状态
  public void RecordAttempt(AttemptRecord attempt)
  {
    Attempts.Add(attempt);
    UpdateMistakeState(attempt);
  }

  private void UpdateMistakeState(AttemptRecord attempt)
  {
    var isWrong = attempt.Result == AttemptResult.Incorrect;
    var isCorrect = attempt.Result == AttemptResult.Correct;

    if (Mistakes.TryGetValue(attempt.QuestionId, out var state))
    {
      state.LastAttemptedAt = attempt.AttemptedAt;
      if (isCorrect)
      {
        state.ConsecutiveCorrectCount++;
      }
      else if (isWrong)
      {
        state.ConsecutiveCorrectCount = 0;
        state.TotalWrongCount++;
        state.IsResolved = false;
      }
    }
    else if (isWrong)
    {
      Mistakes[attempt.QuestionId] = new MistakeState
      {
        QuestionId = attempt.QuestionId,
        FirstWrongAt = attempt.AttemptedAt,
        LastAttemptedAt = attempt.AttemptedAt,
        ConsecutiveCorrectCount = 0,
        TotalWrongCount = 1,
        IsResolved = false
      };
    }
  }

  // 根据阈值，标记已达标的错题为已解决（不物理删除）
  public void ApplyMistakeResolutionThreshold(int threshold)
  {
    foreach (var state in Mistakes.Values)
    {
      if (!state.IsResolved && state.ConsecutiveCorrectCount >= threshold)
      {
        state.IsResolved = true;
      }
    }
  }

  public List<AttemptRecord> GetAttemptsFor(string questionId)
  {
    return Attempts.Where(a => a.QuestionId == questionId).ToList();
  }

  public void UpdateProgress(int lastIndex)
  {
    Progress.LastQuestionIndex = lastIndex;
    Progress.LastUpdatedAt = DateTime.Now;
  }
}