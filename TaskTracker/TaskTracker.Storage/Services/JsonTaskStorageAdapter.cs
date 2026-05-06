using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TaskTracker.Core.Models;
using TaskTracker.Core.Storage;
namespace TaskTracker.Storage.Services;
public class JsonTaskStorageAdapter : ITaskStorage
{
    private readonly JsonTaskStorage _json;
    public JsonTaskStorageAdapter(string filePath)
    {
        _json = new JsonTaskStorage(filePath);
    }
    public List<TaskItem> Load() => _json.Load();
    public void Save(List<TaskItem> tasks) => _json.Save(tasks);
}
