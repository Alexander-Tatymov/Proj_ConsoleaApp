using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json;
namespace TaskTracker.App.Config;
public class ConfigService
{
    private readonly string _configPath;
    public ConfigService(string configPath)
    {
        _configPath = configPath;
    }
    public AppConfig LoadOrCreateDefault()
    {
        if (!File.Exists(_configPath))
        {
            var cfg = new AppConfig();
            Save(cfg);
            return cfg;
        }
        try
        {
            var json = File.ReadAllText(_configPath);
            var cfg = JsonSerializer.Deserialize<AppConfig>(json);
        
return cfg ?? new AppConfig();
        }
        catch
        {
            // если конфиг сломан — создаём новый дефолтный
            var cfg = new AppConfig();
            Save(cfg);
            return cfg;
        }
    }
    public void Save(AppConfig cfg)
    {
        var json = JsonSerializer.Serialize(cfg, new JsonSerializerOptions
    {
            WriteIndented = true
    });
        File.WriteAllText(_configPath, json);
    }
}