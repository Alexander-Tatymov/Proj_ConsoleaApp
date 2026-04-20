using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskTracker.Core.Models;

using static TaskTracker.Core.Services.TaskService;
namespace TaskTracker.Core.Services;
public class TaskService
{
    private readonly List<TaskItem> _tasks;
    private int _nextId;
    public TaskService(List<TaskItem>? initialTasks = null)
    {
        _tasks = initialTasks ?? new List<TaskItem>();
        // следующий Id = максимальный Id + 1
        _nextId = _tasks.Count == 0 ? 1 : _tasks.Max(t => t.Id) + 1;
    }
    public TaskItem Add(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Название не может быть пустым.");
            var task = new TaskItem
            {
                Id = _nextId++,
                Title = title.Trim(),
                Status = Models.TaskStatus.New
            };
        _tasks.Add(task);



        return task;
    }


public List<TaskItem> GetAll()
    {
        return _tasks.ToList();
    }
    private TaskItem GetExisting(int id)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == id);
        if (task is null)
            throw new ArgumentException($"Задача с Id={id} не найдена.");
    return task;
    }
    public TaskItem ChangeStatus(int id, Models.TaskStatus newStatus)
    {
        var task = GetExisting(id);
        task.Status = newStatus;
        var updated = ChangeStatus(id, newStatus);
        return task;
    }
    public void Delete(int id)
    {
        var task = GetExisting(id);
        _tasks.Remove(task);
    }
}