using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskTracker.Storage.Services;

namespace TaskTracker.App.UI;
public static class SafeRunner
{
    public static void Run(string actionName, AppLogger logger, Action action)
    {
        try
        {
            action();
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine("Ошибка: " + ex.Message);
            logger.Error($"{actionName}: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Произошла непредвиденная ошибка.");
            logger.Exception(actionName, ex);
        
        }
    }
}