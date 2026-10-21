using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskTracker.DTO.Common;
using TaskTracker.DTO.Request;
using TaskTracker.DTO.Response;

namespace TaskTracker.Services.Interfaces
{
    public interface ITaskService
    {
        Task<PageResponce<TaskResponse>> GetPageAsync(int projectId, TaskQuery taskQuery, CancellationToken cancellation);
        Task<TaskResponse?> GetByIdAsyc(int id, CancellationToken cancellation);

        Task<TaskResponse> CreateAsync(int projectId, CreateTaskRequest createTaskRequest, CancellationToken cancellation);
        Task<TaskResponse?> UpdateAsync(int id, UpdateTaskRequest updateTaskRequest, CancellationToken cancellation);
        Task<TaskResponse?> UpdateStatusAsync(int id, UpdateTaskStatusRequest request , CancellationToken cancellation);

        Task<bool> DeleteAsync(int id, CancellationToken cancellation);
    }
}
