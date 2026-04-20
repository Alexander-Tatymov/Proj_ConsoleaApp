using TaskTracker.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using static TaskTracker.Core.Services.TaskService;

namespace TaskTracker.Core.Services
{
    public interface ITaskStorage
    {
        void Save(List<TaskItem> tasks);
    }

    public class TaskService : ITaskService
    {
        private List<TaskItem> _tasks;

        public TaskService()
        {
            _tasks = new List<TaskItem>(); // Ключевое исправление!
        }

        public TaskService(List<TaskItem> loadedTasks)
        {
        }

        public List<TaskItem> GetAll()
        {
            return _tasks; // Теперь никогда не null
        }

        public TaskItem Add(string title)
        {
            var task = new TaskItem
            {
                Id = _tasks.Count + 1,
                Title = title,
                IsCompleted = false
            };
            _tasks.Add(task);
            return task;
        }

        public TaskItem ChangeStatus(int id, bool newStatus)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            if (task != null)
            {
                task.IsCompleted = newStatus;
            }
            return task;
        }

        public bool Delete(int id)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            if (task != null)  
            {
                return _tasks.Remove(task);
            }
            return false;

        }

        public interface ITaskService
        {
        }
    }
}