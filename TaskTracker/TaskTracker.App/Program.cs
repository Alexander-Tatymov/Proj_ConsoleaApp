using Microsoft.VisualBasic.FileIO;
using System;
using System.IO;
using System.Threading.Tasks;
using TaskTracker.App.UI;
using TaskTracker.Core.Models;
using TaskTracker.Core.Services;
using TaskTracker.Core.Validation;
using TaskTracker.Storage.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;
using TaskTracker.Core.Storage;
using TaskTracker.App.UI;
using TaskTracker.App.Config;
using TaskTracker.App.Security;
using TaskTracker.Core.Migration;

var baseDir = AppContext.BaseDirectory;
var configPath = Path.Combine(baseDir, "config.json");
var configService = new ConfigService(configPath);
var cfg = configService.LoadOrCreateDefault();
configService.Save(cfg);
Console.WriteLine($"Role: {cfg.Role}");
Console.WriteLine("Config: " + configPath);
Console.WriteLine($"StorageMode: {cfg.StorageMode}, AskOnStart: { cfg.AskOnStart}");

var dataFilePath = Path.Combine(AppContext.BaseDirectory, "data", "tasks.json");
var backupsFolder = Path.Combine(AppContext.BaseDirectory, "backups");
var exportsFolder = Path.Combine(AppContext.BaseDirectory, "exports");
var logsFolder = Path.Combine(AppContext.BaseDirectory, "logs");
var logger = new AppLogger(logsFolder);
logger.Info("Application started");
// Хранилище JSON
string mode = (cfg.StorageMode ?? "Json").Trim();
if (cfg.AskOnStart)
{
    Console.WriteLine("Выберите хранилище:");
    Console.WriteLine("1 - JSON файл (обычный режим)");
    Console.WriteLine("2 - Memory (данные пропадут после выхода)");

Console.Write("Ваш выбор: ");
    var answer = (Console.ReadLine() ?? "").Trim();
    mode = answer == "2" ? "Memory" : "Json";
    // можно сохранить выбор в конфиг (удобно)
    cfg.StorageMode = mode;
    configService.Save(cfg);
}

ITaskStorage storage;
if (string.Equals(mode, "Memory", StringComparison.OrdinalIgnoreCase))
{
    storage = new MemoryTaskStorage();
    Console.WriteLine("Режим: MemoryStorage");
}
else
{
    storage = new JsonTaskStorageAdapter(dataFilePath);
    Console.WriteLine("Режим: JsonStorage");
}

// Загружаем задачи из файла
var loadedTasks = storage.Load();
int changedCount = 0;
for (int i = 0; i < loadedTasks.Count; i++)
{ 
if (TaskNormalizer.Normalize(loadedTasks[i]))
        changedCount++;
}
var service = new TaskService(loadedTasks);
if (changedCount > 0)
{
    storage.Save(service.GetAllActive());
    Console.WriteLine($"Миграция применена. Обновлено задач:{ changedCount}");
logger.Info($"MIGRATION applied changedTasks={changedCount}");
}
else
{
    Console.WriteLine("Миграция не требуется.");
    logger.Info("MIGRATION not needed");
}
Console.WriteLine($"Данные: {dataFilePath}");
Console.WriteLine($"Загружено задач: {loadedTasks.Count}");

logger = new AppLogger(logsFolder);
logger.Info("Application started");

while (true)
{
    Console.WriteLine();
    Console.WriteLine("TaskTracker v0.2");
    ConsoleUi.PrintHeader();
    ConsoleUi.PrintMenu();
    var input = ConsoleUi.ReadString("Выберите пункт меню: ").Trim();

    //var input = Console.ReadLine();
    if (input == "0")
    {
        Console.WriteLine("Выход...");
        break;
    }

    if (input == "1")
    {
        MenuHandlers.AddTask(service, (JsonTaskStorageAdapter)storage, logger);
        continue;
    }

    if (input == "2")
    {
        MenuHandlers.ListTasks(service);
        continue;
    }

    if (input == "3") // Изменить статус задачи
    {
        var tasks = service.GetAllActive();
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

        if (!ConsoleUi.TryReadInt("Введите Id задачи: ", out var id))
        {
            Console.WriteLine("Ошибка: Id должно быть числом.");
            continue;
        }

        Console.WriteLine("Выберите новый статус:");
        Console.WriteLine("0 - New (Новая)");
        Console.WriteLine("1 - InProgress (В работе)");
        Console.WriteLine("2 - Done (Готово)");

        if (!ConsoleUi.TryReadInt("Введите статус (0/1/2): ", out var statusNumber))
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
            storage.Save(service.GetAllActive());
            Console.WriteLine($"Статус изменён: #{updated.Id} {updated.Title} [{updated.Status}]");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine("Ошибка: " + ex.Message);
        }
        continue;
        logger.Info($"STATUS id={updated.Id} newStatus={updated.Status}");
    }

    if (input == "4")
    {
        SafeRunner.Run("DELETE", logger, () =>
        {
            AccessControl.RequireAdmin(cfg.Role);
            MenuHandlers.DeleteTask(service, storage, logger);
        });
        continue;
        object id = null;
        Console.WriteLine($"Задача Id={id} отправлена в корзину.");
        logger.Info($"DELETE_SOFT id={id}");
    }

    if (input == "5")
    {
        var tasks = service.GetAllActive();
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
        if (!ConsoleUi.TryReadInt("Введите Id задачи для редактирования: ",
        out var id))
        {
            Console.WriteLine("Ошибка: Id должно быть числом.");
            continue;
        }
        Console.Write("Введите новое название (Title): ");
        var newTitle = ConsoleUi.ReadString() ?? "";
        Console.Write("Введите новое описание (можно пусто): ");
        var newDescription = ConsoleUi.ReadString() ?? "";
        try
        {
            var updated = service.Update(id, newTitle, newDescription);
            logger.Info($"UPDATE id={updated.Id} title=\"{updated.Title}\"");
            // Сохраняем в JSON после изменения
            storage.Save(service.GetAllActive());
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

        logger.Info($"UPDATE id={updated.Id} title=\"{updated.Title}\"");

    }

    if (input == "6")
    {

        var query = ConsoleUi.ReadString("Введите текст для поиска:");
        var found = service.SearchByTitle(query);
        ConsoleUi.PrintTasks(found);
        continue;
    }

    if (input == "7")
    {
        Console.WriteLine("Выберите статус для фильтра:");
        Console.WriteLine("0 - All (Показать всё)");
        Console.WriteLine("1 - New");
        Console.WriteLine("2 - InProgress");
        Console.WriteLine("3 - Done");
        if (!ConsoleUi.TryReadInt("Введите вариант (0/1/2/3): ", out var option))
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
        ConsoleUi.PrintTasks(filtered);
        continue;
    }

    if (input == "8")
    {
        Console.WriteLine("Выберите сортировку:");
        Console.WriteLine("1 - по Id (по возрастанию)");
        Console.WriteLine("2 - по Id (по убыванию)");
        Console.WriteLine("3 - по статусу, затем по Id");
        if (!ConsoleUi.TryReadInt("Введите вариант (1/2/3): ", out var option))
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
        ConsoleUi.PrintTasks(sorted);
        continue;
    }


    Console.WriteLine("Неизвестная команда. Пожалуйста, выберите пункт из меню.");

    static bool TryReadInt(string prompt, out int value)
    {
        Console.Write(prompt);
        var text = ConsoleUi.ReadString();
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
        var exportPath = ConsoleUi.ReadString();

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
            var tasks = service.GetAllActive();
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
        var importPath = ConsoleUi.ReadString();

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
        // Сохраним текущие данные на всякий случай
        SafeRunner.Run("BACKUP", logger, () =>
        {
            storage.Save(service.GetAllActive());
            var backupPath = BackupService.CreateBackup(dataFilePath, backupsFolder);
            Console.WriteLine("Бэкап создан: " + backupPath);
            logger.Info($"BACKUP created file=\"{backupPath}\"");

        });

        continue;
    }

    if (input == "10")
    {
        SafeRunner.Run("EXPORT", logger, () =>
            {
                Directory.CreateDirectory(exportsFolder);
                var exportFile = Path.Combine(exportsFolder, $"tasks_export_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json");
                var exportStorage = new JsonTaskStorage(exportFile);
                exportStorage.Save(service.GetAllActive());
                Console.WriteLine("Экспорт выполнен: " + exportFile);
                logger.Info($"EXPORT file=\"{exportFile}\"");
            });
        continue;
    }

    Console.WriteLine("Режим импорта:");
    Console.WriteLine("1) Replace — заменить текущие задачи");
    Console.WriteLine("2) Merge — добавить задачи к текущим");
    Console.Write("Ваш выбор (1/2): ");
    var importMode = (Console.ReadLine() ?? "").Trim();
    bool isMerge = importMode == "2";

    if (input == "11")
    {
            AccessControl.RequireAdmin(cfg.Role);

            Console.WriteLine("Импорт заменит текущий список задач!");
            Console.Write("Введите путь к JSON-файлу для импорта: ");
            var importPath = (ConsoleUi.ReadString() ?? "").Trim();
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
            var backupAnswer = (ConsoleUi.ReadString() ?? "").Trim().ToLower();
            if (backupAnswer == "y")
            {
                try
                {
                    storage.Save(service.GetAllActive());
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
            var sure = (ConsoleUi.ReadString() ?? "").Trim().ToLower();
            if (sure != "y")
            {
                Console.WriteLine("Импорт отменён.");
                continue;
            }

            logger.Info($"IMPORT start file=\"{importPath}\"");

            SafeRunner.Run("EXPORT", logger, () =>
            {
                var importStorage = new JsonTaskStorage(importPath);
                var importedTasks = importStorage.Load();
                bool ok = true;
                for (int i = 0; i < importedTasks.Count; i++)
                {
                    var t = importedTasks[i];
                    var error = TaskValidator.Validate(t);
                    if (error != null)
                    {
                        Console.WriteLine($"Ошибка импорта: задача #{i + 1} не прошла проверку: {error}");
                        ok = false;
                        break;
                    }
                }

                if (isMerge)
                {
                    logger.Info($"IMPORT_MERGE start");
                    var result = service.MergeImport(importedTasks);
                    storage.Save(service.GetAllActive());
                    Console.WriteLine($"Merge импорт завершён. Добавлено: {result.added}, пропущено: {result.skipped}");
                    logger.Info($"IMPORT_MERGE success added={result.added} skipped ={ result.skipped}");
}
                else
                {
                    logger.Info($"IMPORT_REPLACE start");
                    service.ReplaceAll(importedTasks);
                    storage.Save(service.GetAllActive());
                    Console.WriteLine($"Replace импорт завершён. Загружено задач: { importedTasks.Count}");
                logger.Info($"IMPORT_REPLACE success count={importedTasks.Count}");
                }
                // Заменяем задачи в сервисе
                service.ReplaceAll(importedTasks);
                // Сохраняем в основной файл data/tasks.json
                storage.Save(service.GetAllActive());
                Console.WriteLine("Импорт выполнен. Загружено задач:" + importedTasks.Count);
                logger.Info($"IMPORT success count={importedTasks.Count}");
            });
            continue;
        }

        if (input == "12")
        {
            SafeRunner.Run("STATS", logger, () =>
            {
                var stats = service.GetStats();
                Console.WriteLine();
                Console.WriteLine("Статистика задач");
                Console.WriteLine("----------------");
                Console.WriteLine($"Всего задач: {stats.Total}");
                Console.WriteLine($"New: {stats.NewCount}");
                Console.WriteLine($"InProgress: {stats.InProgressCount}");
                Console.WriteLine($"Done: {stats.DoneCount}");
                Console.WriteLine("----------------");
                // простая проверка: сумма должна равняться total
                var sum = stats.NewCount + stats.InProgressCount + stats.
            DoneCount;
                if (sum != stats.Total)
                    Console.WriteLine("ВНИМАНИЕ: сумма по статусам не равна общему количеству!");

            });

            continue;
        }

        if (input == "13")
        {
            try
            {
                var reportsFolder = Path.Combine(AppContext.BaseDirectory, "reports");
                Directory.CreateDirectory(reportsFolder);

                var stats = service.GetStats();
                var filePath = Path.Combine(reportsFolder, $"report_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt");

                var lines = new List<string>
            {
                "TaskTracker Report",
                "------------------",
                $"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}","",
                $"Total: {stats.Total}",
                $"New: {stats.NewCount}",
                $"InProgress: {stats.InProgressCount}",
                $"Done: {stats.DoneCount}",
            };

                File.WriteAllLines(filePath, lines);
                Console.WriteLine("Отчёт сохранён: " + filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка экспорта отчёта: " + ex.Message);
            }
            continue;
        }

        if (input == "14")
        {
            SafeRunner.Run("SHOWLOG", logger, () =>
            {
                // Лог-файл за сегодня
                var logFile = Path.Combine(logsFolder, $"app_{DateTime.Now:yyyy-MM-dd}.log");

                var lines = File.ReadAllLines(logFile);
                Console.WriteLine("Последние 20 строк лога:");
                Console.WriteLine("------------------------");
                int start = Math.Max(0, lines.Length - 20);
                for (int i = start; i < lines.Length; i++)
                    Console.WriteLine(lines[i]);

                Console.WriteLine("------------------------");
            });
            continue;
        }

        if (input == "15")
        {
            SafeRunner.Run("SETTINGS", logger, () =>
            {
                AccessControl.RequireAdmin(cfg.Role);

                Console.WriteLine("Текущие настройки:");
                Console.WriteLine($"StorageMode: {cfg.StorageMode}");
                Console.WriteLine($"AskOnStart: {cfg.AskOnStart}");
                Console.WriteLine($"DataFolder: {cfg.DataFolder}");
                Console.WriteLine($"LogsFolder: {cfg.LogsFolder}");
                Console.WriteLine();
                Console.WriteLine("Хотите сменить режим хранения?");
                Console.WriteLine("1 - Json");
                Console.WriteLine("2 - Memory");
                Console.Write("Выбор (Enter - не менять): ");
                var ans = (Console.ReadLine() ?? "").Trim();
                if (ans == "1")
                {
                    cfg.StorageMode = "Json";
                    configService.Save(cfg);
                    Console.WriteLine("Сохранено: StorageMode=Json. Перезапустите приложение.");
                    logger.Info("SETTINGS changed StorageMode=Json");
                }
                else if (ans == "2")
                {
                    cfg.StorageMode = "Memory";
                    configService.Save(cfg);
                    Console.WriteLine("Сохранено: StorageMode=Memory.Перезапустите приложение.");
                    logger.Info("SETTINGS changed StorageMode=Memory");
                }
                else

                {
                    Console.WriteLine("Настройки не изменены.");
                }
            });
            continue;
        }

    if (input == "17")
    {
        SafeRunner.Run("EXPORT_CSV", logger, () =>
        {
            Directory.CreateDirectory(exportsFolder);
            var csvFile = Path.Combine(
            exportsFolder,
            $"tasks_export_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv"
            );
            var csvStorage = new CsvTaskStorage(csvFile);

            csvStorage.Save(service.GetAllActive());
            Console.WriteLine("CSV экспорт выполнен: " + csvFile);
            logger.Info($"EXPORT_CSV file=\"{csvFile}\"");
        });
        continue;
    }

    if (input == "18")
    {
        SafeRunner.Run("IMPORT_CSV", logger, () =>
        {
            AccessControl.RequireAdmin(cfg.Role);
            Console.Write("Введите путь к CSV-файлу: ");
            var csvPath = (Console.ReadLine() ?? "").Trim();
            if (string.IsNullOrWhiteSpace(csvPath))
                throw new ArgumentException("Путь пустой.");
            if (!File.Exists(csvPath))
                throw new ArgumentException("Файл не найден: " +
                csvPath);
            Console.WriteLine("Режим импорта CSV:");
            Console.WriteLine("1) Replace — заменить текущие задачи");
            Console.WriteLine("2) Merge — добавить задачи к текущим");

            Console.Write("Ваш выбор (1/2): ");
            var mode = (Console.ReadLine() ?? "").Trim();
            bool isMerge = mode == "2";
            var csvStorage = new CsvTaskStorage(csvPath);
            var loaded = csvStorage.Load();
            var importedTasks = loaded.tasks;
            var errors = loaded.errors;
            if (isMerge)
            {
                var r = service.MergeImport(importedTasks);
                storage.Save(service.GetAllActive());
                Console.WriteLine($"CSV Merge: добавлено {r.added}, пропущено {r.skipped}, ошибок строк {errors}");
                logger.Info($"IMPORT_CSV_MERGE added={r.added} skipped ={ r.skipped} errors ={ errors}");
            }
            else
            {
                service.ReplaceAll(importedTasks);
                storage.Save(service.GetAllActive());
                Console.WriteLine($"CSV Replace: загружено {importedTasks.Count}, ошибок строк {errors}");
                logger.Info($"IMPORT_CSV_REPLACE count={importedTasks.Count} errors={errors}");
            }
        });
        continue;
    }

    if (input == "19")
    {
        SafeRunner.Run("ADV_FILTER", logger, () =>
        {

            Console.WriteLine("Расширенный фильтр");
            Console.WriteLine("------------------");
            Console.Write("Введите текст (Enter = пусто): ");
            var text = (Console.ReadLine() ?? "").Trim();
            Console.WriteLine("Выберите статус:");
            Console.WriteLine("0 - Любой");
            Console.WriteLine("1 - New");
            Console.WriteLine("2 - InProgress");
            Console.WriteLine("3 - Done");
            Console.Write("Ваш выбор: ");
            var s = (Console.ReadLine() ?? "").Trim();TaskTracker.Core.Models.TaskStatus? status = null;
            if (s == "1") status = TaskTracker.Core.Models.TaskStatus.New;
            else if(s == "2") status = TaskTracker.Core.Models.TaskStatus.InProgress;
            else if(s == "3") status = TaskTracker.Core.Models.TaskStatus.Done;
            var filtered = service.SearchAdvanced(text, status);
            ConsoleUi.PrintTasks(filtered);
            cfg.LastFilterText = text;
            cfg.LastFilterStatus = status.HasValue ? status.Value.ToString() :"Any";
            configService.Save(cfg);
            logger.Info($"ADV_FILTER text=\"{cfg.LastFilterText}\" status={cfg.LastFilterStatus} count={filtered.Count}");
        });
        continue;

}

    if (input == "20")
    {
        SafeRunner.Run("ADV_FILTER_REPEAT", logger, () =>
        {
            var text = cfg.LastFilterText ?? "";
            var statusText = (cfg.LastFilterStatus ?? "Any").Trim();
            TaskTracker.Core.Models.TaskStatus? status = null;
            if (!string.Equals(statusText, "Any", StringComparison.OrdinalIgnoreCase))
            {
                if (Enum.TryParse<TaskTracker.Core.Models.TaskStatus>(statusText, out var parsed))
                    status = parsed;
            }
            Console.WriteLine("Повтор фильтра:");
            Console.WriteLine($"Текст: {text}");
            Console.WriteLine($"Статус: {statusText}");
            var filtered = service.SearchAdvanced(text, status);
            ConsoleUi.PrintTasks(filtered);
            logger.Info($"ADV_FILTER_REPEAT text=\"{text}\" status ={ statusText} count ={ filtered.Count}");
        });
        continue;

}

}

internal class task
{
}