using TaskTracker.App.UI;
using TaskTracker.Core.Models;
using TaskTracker.Core.Services;
using TaskTracker.Core.Storage;
using TaskTracker.Storage.Services;

namespace TaskTracker.App.UI;

public static class MenuHandlers
{
    // Измените JsonTaskStorage на ITaskStorage или JsonTaskStorageAdapter
    public static void AddTask(TaskService service, ITaskStorage storage, AppLogger logger)
    {
        var title = ConsoleUi.ReadString("Введите название задачи: ");
        try
        {
            var task = service.Add(title);
            storage.Save(service.GetAll());
            Console.WriteLine($"Задача добавлена: #{task.Id}{task.Title}[{task.Status}]");
            logger.Info($"ADD id={task.Id} title=\"{task.Title}\"");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine("Ошибка: " + ex.Message);
            logger.Error("ADD failed: " + ex.Message);
        }
    }

    public static void ListTasks(TaskService service)
    {
        var tasks = service.GetAll();
        if (tasks.Count == 0)
        {
            Console.WriteLine("Список задач пуст.");
            return;
        }
        ConsoleUi.PrintTasks(tasks);
    }

    public static void DeleteTask(TaskService service, ITaskStorage storage, AppLogger logger)
    {
        ListTasks(service);
        if (!ConsoleUi.TryReadInt("Введите Id задачи для удаления: ", out var id))
        {
            Console.WriteLine("Ошибка: Id должно быть числом.");
            return;
        }

        var answer = ConsoleUi.ReadString("Точно удалить? (y/n): ").Trim().ToLower();
        if (answer != "y")
        {
            Console.WriteLine("Удаление отменено.");
            return;
        }

        try
        {
            service.Delete(id);
            storage.Save(service.GetAll());
            Console.WriteLine($"Задача с Id={id} удалена.");
            logger.Info($"DELETE id={id}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine("Ошибка: " + ex.Message);
            logger.Error("DELETE failed: " + ex.Message);
        }
    }
}