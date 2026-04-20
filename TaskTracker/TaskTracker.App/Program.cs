using Microsoft.VisualBasic.FileIO;
using System;
using System.IO;
using TaskTracker.Core.Models;
using TaskTracker.Core.Services;
using TaskTracker.Storage.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

var dataFilePath = Path.Combine(AppContext.BaseDirectory, "data", "tasks.json");
var backupsFolder = Path.Combine(AppContext.BaseDirectory, "backups");
var exportsFolder = Path.Combine(AppContext.BaseDirectory, "exports");
// Хранилище JSON
var storage = new JsonTaskStorage(dataFilePath);

// Загружаем задачи из файла
var loadedTasks = storage.Load();
// Создаём сервис с уже загруженными задачами
var service = new TaskService(loadedTasks);
Console.WriteLine($"Данные: {dataFilePath}");
Console.WriteLine($"Загружено задач: {loadedTasks.Count}");

while (true)
{
    Console.WriteLine();
    Console.WriteLine("TaskTracker v0.2");
    Console.WriteLine("----------------");
    Console.WriteLine("0) Выход");
    Console.WriteLine("1) Добавить задачу");
    Console.WriteLine("2) Показать задачи");
    Console.WriteLine("3) Изменить статус задачи");
    Console.WriteLine("4) Удалить задачу");
    Console.WriteLine("5) Редактировать задачу");
    Console.WriteLine("6) Поиск по названию");
    Console.WriteLine("7) Фильтр по статусу");
    Console.WriteLine("8) Сортировка списка");
    Console.WriteLine("9) Сделать резервную копию (backup)");
    Console.WriteLine("10) Экспорт в файл (export)");
    Console.WriteLine("11) Импорт из файла (import)");
    Console.WriteLine("----------------");
    Console.Write("Выберите пункт меню: ");

    var input = Console.ReadLine();
    if (input == "0")
    {
        Console.WriteLine("Выход...");
        break;
    }

    if (input == "1") // Добавить задачу
    {
        Console.Write("Введите название задачи: ");
        string title = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(title))
        {
            Console.WriteLine("Ошибка: название не может быть пустым.");
            continue;
        }

        var task = service.Add(title);
        storage.Save(service.GetAll());
        Console.WriteLine($"Задача добавлена: #{task.Id} {task.Title} [{task.Status}]");
        continue;
    }

    if (input == "2") // Показать задачи
    {

        var tasks = service.GetAll();
        if (tasks.Count == 0)
        {
            Console.WriteLine("Список задач пуст.");
            continue;
        }
        PrintTasks(tasks);
        continue;

        Console.WriteLine("Список задач:");
        foreach (var t in tasks)
        {
            Console.WriteLine($"{t.Id}. {t.Title} [{t.Status}]");
            if (!string.IsNullOrWhiteSpace(t.Description))
                Console.WriteLine($" Описание: {t.Description}");
        }
        continue;
    }

    if (input == "3") // Изменить статус задачи
    {
        var tasks = service.GetAll();
        if (tasks.Count == 0)
        {
            Console.WriteLine("Список задач пуст. Нечего менять.");
            continue;
        }

        Console.WriteLine("Список задач:");
        foreach (var t in tasks)
        {
            Console.WriteLine($"{t.Id}. {t.Title} [{t.Status}]");
        }

        if (!TryReadInt("Введите Id задачи: ", out var id))
        {
            Console.WriteLine("Ошибка: Id должно быть числом.");
            continue;
        }

        Console.WriteLine("Выберите новый статус:");
        Console.WriteLine("0 - New (Новая)");
        Console.WriteLine("1 - InProgress (В работе)");
        Console.WriteLine("2 - Done (Готово)");

        if (!TryReadInt("Введите статус (0/1/2): ", out var statusNumber))
        {
            Console.WriteLine("Ошибка: статус должен быть числом 0 / 1 / 2.");
            continue;
        }

        if (statusNumber < 0 || statusNumber > 2)
        {
            Console.WriteLine("Ошибка: статус должен быть 0, 1 или 2.");
            continue;
        }

        var newStatus = (TaskTracker.Core.Models.TaskStatus)statusNumber;
        try
        {
            var updated = service.ChangeStatus(id, newStatus);
            storage.Save(service.GetAll());
            Console.WriteLine($"Статус изменён: #{updated.Id} {updated.Title} [{updated.Status}]");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine("Ошибка: " + ex.Message);
        }
        continue;
    }

    if (input == "4") // Удалить задачу
    {
        var tasks = service.GetAll();
        if (tasks.Count == 0)
        {
            Console.WriteLine("Список задач пуст. Нечего удалять.");
            continue;
        }

        Console.WriteLine("Список задач:");
        foreach (var t in tasks)
        {
            Console.WriteLine($"{t.Id}. {t.Title} [{t.Status}]");
        }

        if (!TryReadInt("Введите Id задачи для удаления: ", out var id))
        {
            Console.WriteLine("Ошибка: Id должно быть числом.");
            continue;
        }

        Console.Write("Точно удалить? (y/n): ");
        var answer = (Console.ReadLine() ?? "").Trim().ToLower();
        if (answer != "y")
        {
            Console.WriteLine("Удаление отменено.");
            continue;
        }

        try
        {
            service.Delete(id);
            storage.Save(service.GetAll());
            Console.WriteLine($"Задача с Id={id} удалена.");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine("Ошибка: " + ex.Message);
        }
        continue;
    }

    if (input == "5")
    {
        var tasks = service.GetAll();
        if (tasks.Count == 0)
        {
            Console.WriteLine("Список задач пуст. Нечего редактировать.");
        continue;
        }
        Console.WriteLine("Список задач:");
        foreach (var t in tasks)
        {
            Console.WriteLine($"{t.Id}. {t.Title} [{t.Status}]");
            if (!string.IsNullOrWhiteSpace(t.Description))
                Console.WriteLine($" Описание: {t.Description}");
        }
        if (!TryReadInt("Введите Id задачи для редактирования: ",
        out var id))
        {
            Console.WriteLine("Ошибка: Id должно быть числом.");
            continue;
        }
        Console.Write("Введите новое название (Title): ");
        var newTitle = Console.ReadLine() ?? "";
        Console.Write("Введите новое описание (можно пусто): ");
        var newDescription = Console.ReadLine() ?? "";
        try
        {
            var updated = service.Update(id, newTitle, newDescription);
            // Сохраняем в JSON после изменения
            storage.Save(service.GetAll());
            Console.WriteLine("Задача обновлена:");
            Console.WriteLine($"{updated.Id}. {updated.Title} [{updated.Status}]");
            if (!string.IsNullOrWhiteSpace(updated.Description))
                Console.WriteLine($" Описание: {updated.Description}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine("Ошибка: " + ex.Message);
        }
        continue;
    }

    if (input == "6")
    {

            Console.Write("Введите текст для поиска: ");
            var query = Console.ReadLine() ?? "";
            var found = service.SearchByTitle(query);
            PrintTasks(found);
            continue;
    }

    if (input == "7")
    {
        Console.WriteLine("Выберите статус для фильтра:");
        Console.WriteLine("0 - All (Показать всё)");
        Console.WriteLine("1 - New");
        Console.WriteLine("2 - InProgress");
        Console.WriteLine("3 - Done");
        if (!TryReadInt("Введите вариант (0/1/2/3): ", out var option))
        {
            Console.WriteLine("Ошибка: нужно число 0/1/2/3.");
            continue;
        }
        TaskTracker.Core.Models.TaskStatus? status = option switch
        {
            0 => (TaskTracker.Core.Models.TaskStatus?)null,
            1 => TaskTracker.Core.Models.TaskStatus.New,
            2 => TaskTracker.Core.Models.TaskStatus.InProgress,
            3 => TaskTracker.Core.Models.TaskStatus.Done,
            _ => (TaskTracker.Core.Models.TaskStatus?)null
        };


        if (option < 0 || option > 3)
        {
            Console.WriteLine("Ошибка: выберите 0, 1, 2 или 3.");
            continue;
        }
        var filtered = service.FilterByStatus(status);
        PrintTasks(filtered);
        continue;
    }

    if (input == "8")
    {
        Console.WriteLine("Выберите сортировку:");
        Console.WriteLine("1 - по Id (по возрастанию)");
        Console.WriteLine("2 - по Id (по убыванию)");
        Console.WriteLine("3 - по статусу, затем по Id");
        if (!TryReadInt("Введите вариант (1/2/3): ", out var option))
        {
            Console.WriteLine("Ошибка: нужно число 1/2/3.");
            continue;
        }
        List<TaskItem> sorted;
        if (option == 1) sorted = service.SortById(true);
        else if (option == 2) sorted = service.SortById(false);
        else if (option == 3) sorted = service.SortByStatusThenId();
        else
        {
            Console.WriteLine("Ошибка: выберите 1, 2 или 3.");
            continue;
        }
        PrintTasks(sorted);
        continue;
    }


    Console.WriteLine("Неизвестная команда. Пожалуйста, выберите пункт из меню.");

static bool TryReadInt(string prompt, out int value)
{
    Console.Write(prompt);
    var text = Console.ReadLine();
    return int.TryParse(text, out value);
}

static void PrintTasks(List<TaskItem> tasks)
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

    static void ExportTasks(TaskService service)
    {
        Console.Write("Введите путь для экспорта (например: export.json): ");
        var exportPath = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(exportPath))
        {
            Console.WriteLine("Путь не может быть пустым!");
            return;
        }

        try
        {
            // Создаём новый storage для экспорта
            var exportStorage = new JsonTaskStorage(exportPath);

            // Получаем все задачи и сохраняем
            var tasks = service.GetAll();
            exportStorage.Save(tasks);

            Console.WriteLine($"✅ Экспорт выполнен! Задач сохранено: {tasks.Count}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Ошибка при экспорте: {ex.Message}");
        }
    }

    static void ImportTasks(TaskService service)
    {
        Console.Write("Введите путь для импорта (например: import.json): ");
        var importPath = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(importPath))
        {
            Console.WriteLine("Путь не может быть пустым!");
            return;
        }

        try
        {
            // Создаём новый storage для импорта
            var importStorage = new JsonTaskStorage(importPath);

            // Загружаем задачи
            var importedTasks = importStorage.Load();

            if (importedTasks == null || importedTasks.Count == 0)
            {
                Console.WriteLine("Нет задач для импорта или файл пуст!");
                return;
            }

            // Добавляем импортированные задачи в текущий сервис
            foreach (var task in importedTasks)
            {
                // Генерируем новые ID, чтобы избежать конфликтов
                service.Add(task.Title);
                // Можно также добавить описание и статус, если нужно
            }

            Console.WriteLine($"✅ Импорт выполнен! Задач импортировано: {importedTasks.Count}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Ошибка при импорте: {ex.Message}");
        }
    }

    if (input == "9")
    {
        try
        {
            // Сохраним текущие данные на всякий случай
            storage.Save(service.GetAll());
            var backupPath = BackupService.CreateBackup(dataFilePath, backupsFolder);
            Console.WriteLine("Бэкап создан: " + backupPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка backup: " + ex.Message);
        }
        continue;
    }

    if (input == "10")
    {
        try
        {
            Directory.CreateDirectory(exportsFolder);
            var exportFile = Path.Combine(
            exportsFolder,
            $"tasks_export_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json"
            );
            var exportStorage = new JsonTaskStorage(exportFile);
            exportStorage.Save(service.GetAll());
            Console.WriteLine("Экспорт выполнен: " + exportFile);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка export: " + ex.Message);
        }
        continue;
    }

    if (input == "11")
    {
        Console.WriteLine("Импорт заменит текущий список задач!");
        Console.Write("Введите путь к JSON-файлу для импорта: ");
        var importPath = (Console.ReadLine() ?? "").Trim();
        if (string.IsNullOrWhiteSpace(importPath))
        {

        
        Console.WriteLine("Ошибка: путь пустой.");
            continue;
        }
        if (!File.Exists(importPath))
        {
            Console.WriteLine("Ошибка: файл не найден: " + importPath);
            continue;
        }
        Console.Write("Сделать backup перед импортом? (y/n): ");
        var backupAnswer = (Console.ReadLine() ?? "").Trim().ToLower();
        if (backupAnswer == "y")
        {
            try
            {
                storage.Save(service.GetAll());
                var backupPath = BackupService.CreateBackup(dataFilePath, backupsFolder);
                Console.WriteLine("Backup создан: " + backupPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Не удалось сделать backup: " +
                ex.Message);
                Console.WriteLine("Импорт отменён (без backup опасно).");
            continue;
            }
        }
        Console.Write("Точно импортировать и заменить задачи? (y/n): ");
    var sure = (Console.ReadLine() ?? "").Trim().ToLower();
        if (sure != "y")
        {
            Console.WriteLine("Импорт отменён.");
            continue;
        }
        try
        {
            var importStorage = new JsonTaskStorage(importPath);
            var importedTasks = importStorage.Load();
            // Заменяем задачи в сервисе
            service.ReplaceAll(importedTasks);
            // Сохраняем в основной файл data/tasks.json
            storage.Save(service.GetAll());
            Console.WriteLine("Импорт выполнен. Загружено задач:" + importedTasks.Count);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка import: " + ex.Message);
        }
        continue;
    }

}




