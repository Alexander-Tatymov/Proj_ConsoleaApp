using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using TaskTracker.Core.Models;
namespace TaskTracker.Storage.Services;
public class JsonTaskStorage
{
    private readonly string _filePath;
    public JsonTaskStorage(string filePath)
    {
        _filePath = filePath;
    }
    public List<TaskItem> Load()
    {
        // Если файла нет — возвращаем пустой список (это НЕ ошибка)
if (!File.Exists(_filePath))
            return new List<TaskItem>();
        try
        {
            var json = File.ReadAllText(_filePath);
            var tasks = JsonSerializer.Deserialize < List < TaskItem >> (json);
            return tasks ?? new List<TaskItem>();
        }
        catch
        {
            // Если файл повреждён — не падаем, начинаем с пустого списка
        return new List<TaskItem>();
        }
    }
    public void Save(List<TaskItem> tasks)
    {
        var dir = Path.GetDirectoryName(_filePath);
        // Если папки нет — создаём
        if (!string.IsNullOrWhiteSpace(dir))
            Directory.CreateDirectory(dir);
        var json = JsonSerializer.Serialize(tasks, new JsonSerializerOptions
    {
            WriteIndented = true
    });
        File.WriteAllText(_filePath, json);
    }
}