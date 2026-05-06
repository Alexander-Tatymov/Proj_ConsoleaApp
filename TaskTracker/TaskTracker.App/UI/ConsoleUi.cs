using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TaskTracker.Core.Models;
namespace TaskTracker.App.UI;
public static class ConsoleUi
{
    private static object get;

    public static void PrintHeader()
    {
        Console.WriteLine();

        Console.WriteLine("TaskTracker");
        Console.WriteLine("----------------");
    }
    public static void PrintMenu()
    {
        Console.WriteLine("1) Добавить задачу");
        Console.WriteLine("2) Показать список задач");
        Console.WriteLine("3) Изменить статус задачи");
        Console.WriteLine("4) Удалить задачу");
        Console.WriteLine("5) Редактировать задачу");
        Console.WriteLine("6) Поиск по названию");
        Console.WriteLine("7) Фильтр по статусу");
        Console.WriteLine("8) Сортировка списка");
        Console.WriteLine("9) Сделать резервную копию (backup)");
        Console.WriteLine("10) Экспорт в файл (export)");
        Console.WriteLine("11) Импорт из файла (import)");
        Console.WriteLine("12) Статистика (отчёт)");
        Console.WriteLine("13) Экспорт отчёта в файл");
        Console.WriteLine("14) Показать последние строки лога");
        Console.WriteLine("0) Выход");
        Console.WriteLine("----------------");
    }
    public static string ReadString(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine() ?? "";
    }
    public static bool TryReadInt(string prompt, out int value)
    {
        Console.Write(prompt);



        var text = Console.ReadLine();
        return int.TryParse(text, out value);
    }
    public static void PrintTasks(List<TaskItem> tasks)
    {
        if (tasks.Count == 0)
        {
            Console.WriteLine("Ничего не найдено.");
            return;
        }
        Console.WriteLine("Список задач:");
        foreach (var t in tasks)
        {
            Console.WriteLine($"{t.Id}. {t.Title} [{t.Status}]");
            if (!string.IsNullOrWhiteSpace(t.Description))
                Console.WriteLine($" Описание: {t.Description}");
        }
    }

    internal static string? ReadString()
    {
        return Console.ReadLine();
    }
}