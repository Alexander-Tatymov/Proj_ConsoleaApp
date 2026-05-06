using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Linq;
using TaskTracker.Core.Models;
namespace TaskTracker.Core.Storage;
public class MemoryTaskStorage : ITaskStorage
{
    private List<TaskItem> _tasks = new();
    public List<TaskItem> Load()
    {
        // Возвращаем копию
        return _tasks.Select(t => new TaskItem
        {
            Id = t.Id,
            Title = t.Title,

        
Description = t.Description,
            Status = t.Status
        }).ToList();
    }
    public void Save(List<TaskItem> tasks)
    {
        // Сохраняем копию (защита от случайных изменений извне)
_tasks = tasks.Select(t => new TaskItem
{
    Id = t.Id,
    Title = t.Title,
    Description = t.Description,
    Status = t.Status
}).ToList();
    }
}