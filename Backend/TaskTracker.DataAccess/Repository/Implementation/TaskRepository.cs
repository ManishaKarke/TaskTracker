using Microsoft.EntityFrameworkCore;
using TaskTracker.DataAccess.Context;
using TaskTracker.DataAccess.Repository.Interfaces;
using TaskTracker.Model.Entities;
using TaskTracker.Model.Enums;

namespace TaskTracker.DataAccess.Repository.Implementation
{
    public class TaskRepository : ITaskRepository
    {
        private readonly ApplicationDbContext _context;

        public TaskRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Delete(TaskItem taskItem)
        {
            _context.TaskItems.Remove(taskItem);
        }

        public async Task<TaskItem?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _context.TaskItems
               .AsNoTracking()
               .FirstOrDefaultAsync(
                   task => task.Id == id,
                   cancellationToken);

        }

        public async Task<(List<TaskItem> Items, int TotalCount)> GetPagedAsync(int projectId, Model.Enums.TaskStatus? status, TaskPriority? priority, string? search, string sortBy, string sortDir, int page, int pageSize, CancellationToken cancellationToken)
        {
            var query = _context.TaskItems.AsNoTracking().Where(task => task.ProjectId == projectId);
            if (status.HasValue)
                query = query.Where(task => task.Status == status.Value);

            if(priority.HasValue)
                query = query.Where(task => task.Priority == priority.Value);

            if(!string.IsNullOrWhiteSpace(search))
            {
                var searchItem = search.Trim();
                query = query.Where(task => task.Title.Contains(searchItem));
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var desceding = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);

            query = sortBy.ToLowerInvariant() switch
            {
                "duedate" => desceding
                    ? query.OrderByDescending(task => task.DueDate)
                           .ThenBy(task => task.Id)
                    : query.OrderBy(task => task.DueDate)
                           .ThenBy(task => task.Id),

                "priority" => desceding
                    ? query.OrderByDescending(task => task.Priority)
                           .ThenBy(task => task.Id)
                    : query.OrderBy(task => task.Priority)
                           .ThenBy(task => task.Id),

                _ => desceding
                    ? query.OrderByDescending(task => task.CreatedAt)
                           .ThenBy(task => task.Id)
                    : query.OrderBy(task => task.CreatedAt)
                           .ThenBy(task => task.Id)
            };

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);


        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        public async Task AddAsync(TaskItem taskItem, CancellationToken cancellationToken)
        {
            await _context.TaskItems.AddAsync(taskItem,  cancellationToken);
        }

        public void Update(TaskItem taskItem)
        {
            _context.TaskItems.Update(taskItem);
        }

        public async Task<TaskItem?> GetByIdForUpdateAsync(int id,CancellationToken cancellationToken)
        {
            return await _context.TaskItems.FirstOrDefaultAsync(task => task.Id == id,cancellationToken);
        }

        public void UpdateWithConcurrency(TaskItem taskItem,byte[] originalRowVersion)
        {
            var entry = _context.Entry(taskItem);

            entry.State = EntityState.Modified;

            entry.Property(task => task.RowVersion).OriginalValue = originalRowVersion;

            entry.Property(task => task.RowVersion).IsModified = false;
        }

    }


}
