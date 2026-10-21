using Azure.Core;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using TaskTracker.DataAccess.Repository.Interfaces;
using TaskTracker.DTO.Common;
using TaskTracker.DTO.Request;
using TaskTracker.DTO.Response;
using TaskTracker.Model.Entities;
using TaskTracker.Model.Enums;
using TaskTracker.Services.Exceptions;
using TaskTracker.Services.Interfaces;
using TaskStatus = TaskTracker.Model.Enums.TaskStatus;

namespace TaskTracker.Services.Implementations
{
    public class TaskService : ITaskService
    {

        private readonly ITaskRepository _taskRepository;
        private readonly IProjectRepository _projectRepository;

        public TaskService(ITaskRepository taskRepository, IProjectRepository projectRepository)
        {
            _taskRepository = taskRepository;
            _projectRepository = projectRepository;
        }
        public async Task<TaskResponse> CreateAsync(int projectId, CreateTaskRequest createTaskRequest, CancellationToken cancellation)
        {
            ValidateId(projectId, "Project");
            ValidateTaskFields(
                createTaskRequest.Title,
                createTaskRequest.Description,
                createTaskRequest.AssignedTo,
                createTaskRequest.Status,
                createTaskRequest.Priority);

            if (createTaskRequest.DueDate.HasValue &&
                createTaskRequest.DueDate.Value.Date < DateTime.UtcNow.Date)
            {
                throw new ServiceExceptioncs(
                    400, "invalid_due_date",
                    "Due date cannot be in the past.");
            }

            var project = await _projectRepository.GetByIdAsync(projectId, cancellation);

            if (project is null)
            {
                throw new ServiceExceptioncs(
                    404, "project_not_found",
                    "Cannot create a task because the project was not found.");
            }


            var task = new TaskItem
            {
                ProjectId = projectId,
                Title = createTaskRequest.Title.Trim(),
                Description = NormalizeOptionalText(createTaskRequest.Description),
                Status = createTaskRequest.Status,
                Priority = createTaskRequest.Priority,
                AssignedTo = NormalizeOptionalText(createTaskRequest.AssignedTo),
                DueDate = createTaskRequest.DueDate
            };

            await _taskRepository.AddAsync(task, cancellation);
            await _taskRepository.SaveChangesAsync(cancellation);

            return MapToResponse(task);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellation)
        {
            ValidateId(id, "Task");

            var task = await _taskRepository.GetByIdForUpdateAsync(id, cancellation);

            if (task is null)
            {
                return false;
            }

            _taskRepository.Delete(task);
            await _taskRepository.SaveChangesAsync(cancellation);

            return true;
        }

        public async Task<TaskResponse?> GetByIdAsyc(int id, CancellationToken cancellation)
        {
            ValidateId(id, "Task");

            var task = await _taskRepository.GetByIdAsync(id, cancellation);

            return task is null ? null : MapToResponse(task);
        }

        public async Task<PageResponce<TaskResponse>> GetPageAsync(int projectId, TaskQuery taskQuery, CancellationToken cancellation)
        {
            if (projectId <= 0)
            {
                throw new ServiceExceptioncs(
                    400, "invalid_project_id",
                    "Project ID must be greater than zero.");
            }

            ValidateQuery(taskQuery);

            var project = await _projectRepository.GetByIdAsync(
                projectId, cancellation);

            if (project is null)
            {
                throw new ServiceExceptioncs(404, "project_not_found", "Project was not found.");
            }

            var result = await _taskRepository.GetPagedAsync(
                projectId,
                taskQuery.Status,
                taskQuery.Priority,
                taskQuery.Search,
                taskQuery.SortBy,
                taskQuery.SortDir,
                taskQuery.Page,
                taskQuery.PageSize,
                cancellation);

            return new PageResponce<TaskResponse>
            {
                Items = result.Items.Select(MapToResponse).ToList(),
                Page = taskQuery.Page,
                PageSize = taskQuery.PageSize,
                TotalCount = result.TotalCount
            };
        }

        public async Task<TaskResponse?> UpdateAsync(int id, UpdateTaskRequest updateTaskRequest, CancellationToken cancellation)
        {
            ValidateId(id, "Task");

            ValidateTaskFields(
                updateTaskRequest.Title,
                updateTaskRequest.Description,
                updateTaskRequest.AssignedTo,
                updateTaskRequest.Status,
                updateTaskRequest.Priority);

            var clientRowVersion = DecodeRowVersion(updateTaskRequest.RowVersion);

            var task = await _taskRepository.GetByIdForUpdateAsync(
                id, cancellation);

            if (task is null)
            {
                return null;
            }

            task.Title = updateTaskRequest.Title.Trim();
            task.Description = NormalizeOptionalText(updateTaskRequest.Description);
            task.Status = updateTaskRequest.Status;
            task.Priority = updateTaskRequest.Priority;
            task.AssignedTo = NormalizeOptionalText(updateTaskRequest.AssignedTo);
            task.DueDate = updateTaskRequest.DueDate;

            _taskRepository.UpdateWithConcurrency(task, clientRowVersion);

            try
            {
                await _taskRepository.SaveChangesAsync(cancellation);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ServiceExceptioncs( 409, "concurrency_conflict","This task was modified by another user. Reload it before updating.");
            }

            return MapToResponse(task);
        }

        public async Task<TaskResponse?> UpdateStatusAsync(int id, UpdateTaskStatusRequest request, CancellationToken cancellation)
        {
            ValidateId(id, "Task");

            if (!Enum.IsDefined(typeof(TaskStatus), request.Status))
            {
                throw new ServiceExceptioncs(
                    400, "invalid_status", "The specified task status is invalid.");
            }

            var task = await _taskRepository.GetByIdForUpdateAsync(id, cancellation);

            if (task is null)
            {
                return null;
            }

            var clientRowVersion = DecodeRowVersion(request.RowVersion);

            task.Status = request.Status;

            _taskRepository.UpdateWithConcurrency(task, clientRowVersion);

            try
            {
                await _taskRepository.SaveChangesAsync(cancellation);
            }

            catch (DbUpdateConcurrencyException)
            {
                throw new ServiceExceptioncs(
                    409, "concurrency_conflict",
                    "This task was modified by another user. Reload it before changing its status.");
            }

            return MapToResponse(task);

        }

        private static void ValidateQuery(TaskQuery query)
        {
            if (query.Page < 1)
            {
                throw new ServiceExceptioncs(
                    400, "invalid_page", "Page must be at least 1.");
            }

            if (query.PageSize is < 1 or > 50)
            {
                throw new ServiceExceptioncs(
                    400, "invalid_page_size",
                    "PageSize must be between 1 and 50.");
            }

            if (!new[] { "dueDate", "priority", "createdAt" }
                .Contains(query.SortBy, StringComparer.OrdinalIgnoreCase))
            {
                throw new ServiceExceptioncs(
                    400, "invalid_sort_by",
                    "sortBy must be dueDate, priority, or createdAt.");
            }

            if (!new[] { "asc", "desc" }.Contains(query.SortDir, StringComparer.OrdinalIgnoreCase))
            {
                throw new ServiceExceptioncs(
                    400, "invalid_sort_direction",
                    "sortDir must be asc or desc.");
            }

            if (query.Status.HasValue &&
                !Enum.IsDefined(typeof(TaskStatus), query.Status.Value))
            {
                throw new ServiceExceptioncs(
                    400, "invalid_status", "Invalid task status.");
            }

            if (query.Priority.HasValue &&
                !Enum.IsDefined(typeof(TaskPriority), query.Priority.Value))
            {
                throw new ServiceExceptioncs(
                    400, "invalid_priority", "Invalid task priority.");
            }
        }

        private static void ValidateTaskFields(
            string? title,
            string? description,
            string? assignedTo,
            TaskStatus status,
            TaskPriority priority)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ServiceExceptioncs(
                    400, "validation_error", "Task title is required.");
            }

            if (title.Trim().Length > 200)
            {
                throw new ServiceExceptioncs(
                    400, "validation_error",
                    "Task title cannot exceed 200 characters.");
            }

            if (description?.Length > 1000)
            {
                throw new ServiceExceptioncs(
                    400, "validation_error",
                    "Task description cannot exceed 1000 characters.");
            }

            if (assignedTo?.Length > 100)
            {
                throw new ServiceExceptioncs(
                    400, "validation_error",
                    "Assignee cannot exceed 100 characters.");
            }

            if (!Enum.IsDefined(typeof(TaskStatus), status))
            {
                throw new ServiceExceptioncs(
                    400, "invalid_status", "Invalid task status.");
            }

            if (!Enum.IsDefined(typeof(TaskPriority), priority))
            {
                throw new ServiceExceptioncs(
                    400, "invalid_priority", "Invalid task priority.");
            }
        }

        private static void ValidateId(int id, string entityName)
        {
            if (id <= 0)
            {
                throw new ServiceExceptioncs(
                    400, "invalid_id",
                    $"{entityName} ID must be greater than zero.");
            }
        }

        private static string? NormalizeOptionalText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static byte[] DecodeRowVersion(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ServiceExceptioncs(
                    400, "validation_error", "RowVersion is required.");
            }

            try
            {
                var rowVersion = Convert.FromBase64String(value);

                if (rowVersion.Length != 8)
                {
                    throw new ServiceExceptioncs(
                        400, "invalid_row_version",
                        "RowVersion must be a valid SQL Server row version.");
                }

                return rowVersion;
            }
            catch (FormatException)
            {
                throw new ServiceExceptioncs(
                    400, "invalid_row_version",
                    "RowVersion must be a valid Base64 string.");
            }
        }

        private static TaskResponse MapToResponse(TaskItem task)
        {
            return new TaskResponse
            {
                Id = task.Id,
                ProjectId = task.ProjectId,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                priority = task.Priority,
                AssignedTo = task.AssignedTo,
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt,
                Rowversion = Convert.ToBase64String(task.RowVersion)
            };
        }
    }
}
