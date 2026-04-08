
using TaskTracker.Core.Models;
using TaskStatus = TaskTracker.Core.Models.TaskStatus;
var tasks = new List<TaskItem>();
var nextId = 1;
while (true)
{
    Console.WriteLine();
    Console.WriteLine("TaskTracker v0.2");
    Console.WriteLine("----------------");
    Console.WriteLine("1) Добавить задачу");
    Console.WriteLine("2) Показать список задач");
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