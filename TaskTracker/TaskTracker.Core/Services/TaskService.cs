using System.Threading.Tasks;
using TaskTracker.Core.Models;
using TaskStatus = TaskTracker.Core.Models.TaskStatus;
namespace TaskTracker.Core.Services;
public class TaskService
{
    private readonly List<TaskItem> _tasks = new();
    private int _nextId = 1;
    public TaskItem Add(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Название не может быть пустым.");
            var task = new TaskItem
            {
                Id = _nextId++,
                Title = title.Trim(),
                Status = TaskStatus.New
            };
        _tasks.Add(task);


    
return task;
    }

    public object Add(object title)
    {
        throw new NotImplementedException();
    }

    public List<TaskItem> GetAll()
    {
        // Возвращаем копию, чтобы внешний код не ломал список
    return _tasks.ToList();
    }
}