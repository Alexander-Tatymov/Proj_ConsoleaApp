using TaskTracker.Core.Models;
namespace TaskTracker.Core.Storage;
public interface ITaskStorage
{
    List<TaskItem> Load();
    void Save(List<TaskItem> tasks);
}