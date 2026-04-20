using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskTracker.Core.Models;
    public class TaskItem
    {
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = ""; // ← добавили
    public TaskStatus Status { get; set; } = TaskStatus.New;

    public bool IsCompleted { get; internal set; }

    public void Add(TaskItem task)
    {
        throw new NotImplementedException();
    }
    }