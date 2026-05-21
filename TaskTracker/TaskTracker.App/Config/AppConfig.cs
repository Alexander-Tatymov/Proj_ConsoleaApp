using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskTracker.App.Config;
public class AppConfig
{
    public string StorageMode { get; set; } = "Json"; // "Json" или "Memory"
public bool AskOnStart { get; set; } = false;
    public string DataFolder { get; set; } = "data";
    public string LogsFolder { get; set; } = "logs";
    public string BackupsFolder { get; set; } = "backups";
    public string ExportsFolder { get; set; } = "exports";
    public string ReportsFolder { get; set; } = "reports";
    public string Role { get; set; } = "User";
    public string LastFilterText { get; set; } = "";
    public string LastFilterStatus { get; set; } = "Any";
}
