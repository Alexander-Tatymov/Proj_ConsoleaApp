using TaskTracker.Core.Services;
using TaskTracker.Core.Models;
using TaskStatus = TaskTracker.Core.Models.TaskStatus;
var service = new TaskService();
while (true)
{
    Console.WriteLine();
    Console.WriteLine("TaskTracker v0.2");
    Console.WriteLine("----------------");


    try
    {
        string title = Console.ReadLine();
        var task = service.Add(title);
        Console.WriteLine($"Задача добавлена: #{task.Id} {task.Title} [{task.Status}]");
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine("Ошибка: " + ex.Message);
    }


    var tasks = service.GetAll();
    Console.WriteLine("0) Выход");
    Console.WriteLine("----------------");
    Console.Write("Выберите пункт меню: ");
    var input = Console.ReadLine();
    if (input == "0")
    {
        Console.WriteLine("Выход...");
        break;
    }
    if (input == "1")
    {
        Console.Write("Введите название задачи: ");
        var title = Console.ReadLine() ?? "";

    
if (string.IsNullOrWhiteSpace(title))
        {
            Console.WriteLine("Ошибка: название не может быть пустым.");
continue;
        }
        int nextId = 0;
        var task = new TaskItem
        {
            Id = nextId,
            Title = title.Trim(),
            Status = TaskStatus.New
        };
        nextId++;
        tasks.Add(task);
        Console.WriteLine($"Задача добавлена: #{task.Id} {task.Title} [{task.Status}]");
        continue;
    }
    if (input == "2")
    {
        if (tasks.Count == 0)
        {
            Console.WriteLine("Список задач пуст.");
            continue;
        }
        Console.WriteLine("Список задач:");
        foreach (var t in tasks)
        {
            Console.WriteLine($"{t.Id}. {t.Title} [{t.Status}]");        }
    
continue;
    }
    Console.WriteLine("Неизвестная команда. Введите 1, 2 или 0.");
}