using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TaskTracker.Core.Models;
using TaskTracker.Core.Validation;
namespace TaskTracker.Core.Migration;
public static class TaskNormalizer
{
    public static bool Normalize(TaskItem t)
    {
        bool changed = false;

        if (t.Title == null)
        {
            t.Title = "";
            changed = true;
        }

        if (t.Description == null)
        {
            t.Description = "";
            changed = true;
        }

        // Trim для Title
        var titleTrim = t.Title.Trim();
        if (titleTrim != t.Title)
        {
            t.Title = titleTrim;
            changed = true;
        }

        // Trim для Description
        var descTrim = t.Description.Trim();
        if (descTrim != t.Description)
        {
            t.Description = descTrim;
            changed = true;
        }

        // Обрезка Title по длине
        if (t.Title.Length > TaskValidator.TitleMaxLength)
        {
            t.Title = t.Title.Substring(0, TaskValidator.TitleMaxLength);
            changed = true;
        }

        // Обрезка Description по длине
        if (t.Description.Length > TaskValidator.DescriptionMaxLength)
        {
            t.Description = t.Description.Substring(0, TaskValidator.DescriptionMaxLength);
            changed = true;
        }

        return changed; // ← Теперь return гарантирован

}
}

