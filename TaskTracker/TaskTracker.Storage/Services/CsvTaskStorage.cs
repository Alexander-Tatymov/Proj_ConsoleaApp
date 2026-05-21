using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskTracker.Core.Models;
namespace TaskTracker.Storage.Services;
public class CsvTaskStorage
{
    private readonly string _filePath;
    public CsvTaskStorage(string filePath)
    {
        _filePath = filePath;
    }
    public void Save(List<TaskItem> tasks)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        var lines = new List<string>
{
"Id,Title,Description,Status"
};
        foreach (var t in tasks)
        {
            var id = t.Id.ToString();
            var title = EscapeCsv(t.Title ?? "");
        
var desc = EscapeCsv(t.Description ?? "");
            var status = t.Status.ToString();
            lines.Add($"{id},{title},{desc},{status}");
        }
        File.WriteAllLines(_filePath, lines);
    }
    public (List<TaskItem> tasks, int errors) Load()
    {
        if (!File.Exists(_filePath))
            throw new FileNotFoundException("CSV файл не найден.", _filePath); var lines = File.ReadAllLines(_filePath).ToList();
        var result = new List<TaskItem>();
        int errors = 0;
        if (lines.Count == 0)
            return (result, 0);
        int start = 0;
        if (lines[0].StartsWith("Id,", StringComparison.OrdinalIgnoreCase))
            start = 1;
        for (int i = start; i < lines.Count; i++)
        {
            var line = lines[i].Trim();
            if (line.Length == 0) continue;
            try
            {
                var parts = ParseCsvLine(line);
                if (parts.Count < 4)
{
                    errors++;
                    continue;
                }
                if (!int.TryParse(parts[0], out var id))
                {
                    errors++;
                    continue;
                }
                var title = parts[1];
                var desc = parts[2];
                var statusText = parts[3];
                if (!Enum.TryParse<Core.Models.TaskStatus>(statusText, out var status))
                {
                    if (int.TryParse(statusText, out var sNum) && sNum >= 0 && sNum <= 2) 
                        status = (Core.Models.TaskStatus)sNum;
                    else
                    {
                        errors++;
                        continue;

                    }
                }
                result.Add(new TaskItem
                {
                    Id = id,
                    Title = title,
                    Description = desc,
                    Status = status
                });
            }

        
catch
{
                errors++;
            }
        }
        return (result, errors);
    }
    private static string EscapeCsv(string text)
    {
        bool mustQuote = text.Contains(',') || text.Contains
        ('"') || text.Contains('\n') || text.Contains('\r');
        if (text.Contains('"'))
            text = text.Replace("\"", "\"\"");
        return mustQuote ? $"\"{text}\"" : text;
    }
    private static List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        var current = "";
        bool inQuotes = false;
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] =='"')
                
{
                        current += '"';

                        i++;
                    }
else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    current += c;
                }
            }
            else
            {
                if (c == '"')
                {
                    inQuotes = true;
                }
                else if (c == ',')
                {
                    result.Add(current);
                    current = "";
                }
                else
                {
                    current += c;
                }
            }
        }
        result.Add(current);
        return result;
}
}