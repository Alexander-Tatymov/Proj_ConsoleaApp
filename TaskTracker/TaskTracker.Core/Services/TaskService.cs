using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskTracker.Core.Models;
using static TaskTracker.Core.Services.TaskService;


namespace TaskTracker.Core.Services;
public class TaskService
{

    public List<TaskItem> SearchByTitle(string query)
    {
        query ??= "";
        query = query.Trim();
        if (query.Length == 0)
            return GetAll();
        return _tasks
        .Where(t => (t.Title ?? "").Contains(query, StringComparison.OrdinalIgnoreCase))
        .ToList();
    }

    public List<TaskItem> FilterByStatus(Models.TaskStatus? status)
    {
        if (status is null)
            return GetAll(); // null = All
        return _tasks.Where(t => t.Status == status).ToList();
    }

    public List<TaskItem> SortById(bool ascending = true)
    {
        return ascending
        ? _tasks.OrderBy(t => t.Id).ToList()
        : _tasks.OrderByDescending(t => t.Id).ToList();
    }

    public List<TaskItem> SortByStatusThenId()
    {
        return _tasks
        .OrderBy(t => t.Status)
        .ThenBy(t => t.Id)
        .ToList();
    }


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
    public TaskItem Update(int id, string newTitle, string newDescription)
    {
        if (string.IsNullOrWhiteSpace(newTitle))
            throw new ArgumentException("Название не может быть пустым.");
        var task = GetExisting(id);
        task.Title = newTitle.Trim();
        task.Description = (newDescription ?? "").Trim();
        return task;
    }


    public List<TaskItem> GetAll()
    {
        return _tasks.ToList();
    }
    private TaskItem GetExisting(int id)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == id);
        if (task is null)  throw new ArgumentException($"Задача с Id={id} не найдена.");
        return task;
    }

    public TaskItem ChangeStatus(int id, Models.TaskStatus newStatus)
    {
        var task = GetExisting(id);
        task.Status = newStatus;
        return task;
    }
    public void Delete(int id)
    {
        var task = GetExisting(id);
        _tasks.Remove(task);
    }

}