using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskTracker.Core.Models;
using static TaskTracker.Core.Services.TaskService;
using TaskTracker.Core.Validation;
using TaskTracker.Core.Reports;
using TaskStatus = TaskTracker.Core.Models.TaskStatus;


namespace TaskTracker.Core.Services;
public class TaskService
{

    private int GetNextFreeId()
    {
        while (_tasks.Any(t => t.Id == _nextId))
            _nextId++;
        return _nextId++;
    }

    public (int added, int skipped) MergeImport(List<TaskItem> imported)
    {

        imported ??= new List<TaskItem>();
        int added = 0;
        int skipped = 0;
        foreach (var t in imported)
        {
            if (t == null)
            {
                skipped++;
                continue;
            }
            bool duplicate = _tasks.Any(x =>
            string.Equals(x.Title, t.Title, StringComparison.
            OrdinalIgnoreCase) &&
            string.Equals(x.Description, t.Description, StringComparison.OrdinalIgnoreCase) &&
            x.Status == t.Status
            );
            if (duplicate)
            {
                skipped++;
                continue;
            }
            int newId = t.Id;
            if (_tasks.Any(x => x.Id == newId))
                newId = GetNextFreeId();
            var copy = new TaskItem
            {
                Id = newId,
                Title = t.Title ?? "",
                Description = t.Description ?? "",
            Status = t.Status
            };
            _tasks.Add(copy);
            added++;
        }
        _nextId = _tasks.Count == 0 ? 1 : _tasks.Max(x => x.Id) +
        1;
        return (added, skipped);
    }

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
        var task = new TaskItem
        {
            Id = _nextId,
            Title = title ?? "",
            Description = "",
            Status = Models.TaskStatus.New
        };
        var error = TaskValidator.Validate(task);
        if (error != null)
            throw new ArgumentException(error);
        task.Title = task.Title.Trim();
        _tasks.Add(task);
        _nextId++;
            return task;
    }
    public TaskItem Update(int id, string newTitle, string newDescription)
    {
        var task = GetExisting(id);
        // временно присваиваем новые значения
        task.Title = newTitle ?? "";
        task.Description = newDescription ?? "";
        var error = TaskValidator.Validate(task);
        if (error != null)
            throw new ArgumentException(error);
        // нормализуем (Trim)
        task.Title = task.Title.Trim();
        task.Description = (task.Description ?? "").Trim();
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

    public void ReplaceAll(List<TaskItem> newTasks)
    {
        newTasks ??= new List<TaskItem>();
        _tasks.Clear();
        _tasks.AddRange(newTasks);
        // Пересчитать следующий Id
        _nextId = _tasks.Count == 0 ? 1 : _tasks.Max(t => t.Id) + 1;
    }

    public TaskStats GetStats()
    {
        var stats = new TaskStats();
        stats.Total = _tasks.Count;
        foreach (var t in _tasks)
        {
            if (t.Status == TaskStatus.New) stats.NewCount++;
            else if (t.Status == TaskStatus.InProgress) stats.InProgressCount++;
            else if (t.Status == TaskStatus.Done) stats.DoneCount++;
        }
        return stats;
    }

    public List<TaskItem> SearchAdvanced(string? text, TaskStatus?
status)
    {
        var query = (text ?? "").Trim();
        var hasText = query.Length > 0;
        var result = new List<TaskItem>();
        foreach (var t in _tasks)
        {
            bool ok = true;
            if (status.HasValue && t.Status != status.Value)
                ok = false;
            if (ok && hasText)
            {
                var inTitle = (t.Title ?? "").Contains(query, StringComparison.OrdinalIgnoreCase);
                var inDesc = (t.Description ?? "").Contains(query, StringComparison.OrdinalIgnoreCase);
                if (!inTitle && !inDesc)
                    ok = false;
            }
            if (ok)
                result.Add(t);
        }
        return result;
    }

}