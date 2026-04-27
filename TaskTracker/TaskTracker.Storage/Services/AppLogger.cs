using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskTracker.Storage.Services;
public class AppLogger
{
    private readonly string _logDir;
    public AppLogger(string logDir)
    {
        _logDir = logDir;
        Directory.CreateDirectory(_logDir);
    }
    private string GetLogFilePath()
    {
        // один файл на день
        var fileName = $"app_{DateTime.Now:yyyy-MM-dd}.log";
    
    return Path.Combine(_logDir, fileName);
    }
    public void Info(string message)
    {
        var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} INFO{ message}";
    WriteLine(line);
    }
    public void Error(string message)
    {
        var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} ERROR{ message}";
    WriteLine(line);
    }
    private void WriteLine(string line)
    {
        var path = GetLogFilePath();
        File.AppendAllText(path, line + Environment.NewLine);
    }
}