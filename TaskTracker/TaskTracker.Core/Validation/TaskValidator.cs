using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskTracker.Core.Models;

namespace TaskTracker.Core.Validation;
public static class TaskValidator
{
    public const int TitleMaxLength = 50;
    public const int DescriptionMaxLength = 300;


// Возвращает null если всё хорошо, иначе текст ошибки
public static string? Validate(TaskItem task)
    {
        if (task is null)
            return "Задача не должна быть пустой (null).";
        // Title
        if (string.IsNullOrWhiteSpace(task.Title))
            return "Название (Title) обязательно.";
        var title = task.Title.Trim();
        if (title.Length > TitleMaxLength)
            return $"Название слишком длинное. Максимум {TitleMaxLength} символов.";
        // Description
        var desc = (task.Description ?? "").Trim();
        if (desc.Length > DescriptionMaxLength)
            return $"Описание слишком длинное. Максимум {DescriptionMaxLength} символов.";
        return null;
    }
}