using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskTracker.Model.Entities;
using TaskTracker.Model.Enums;
using TaskStatus = TaskTracker.Model.Enums.TaskStatus;

namespace TaskTracker.DataAccess.Repository.Interfaces
{
    public interface ITaskRepository
    {
        Task<(List<TaskItem> Items, int TotalCount)> GetPagedAsync(int projectId, TaskStatus? status, TaskPriority? priority, string? search, string sortBy, string sortDir, int page, int pageSize, CancellationToken cancellationToken);
        Task<TaskItem?> GetByIdAsync(int id, CancellationToken cancellationToken);

        void Update(TaskItem taskItem);
        void Delete(TaskItem taskItem);

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);


    }
}
