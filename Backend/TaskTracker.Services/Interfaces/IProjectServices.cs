using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskTracker.DTO.Request;
using TaskTracker.DTO.Response;
using TaskTracker.Model.Entities;

namespace TaskTracker.Services.Interfaces
{
    public interface IProjectServices
    {
        Task<IReadOnlyList<ProjectResponseResponse>> GetAllAsync(CancellationToken cancellation);

        Task<ProjectResponseResponse> GetByIdAsync(int id, CancellationToken cancellation);

        Task<ProjectResponseResponse> CreateAsync(CreateProjectRequest createProjectRequest, CancellationToken cancellation);
        Task<ProjectResponseResponse?> UpdateAsync(int id, UpdateProjectRequest updateProjectRequest, CancellationToken cancellation);

        Task<bool> DeleteAsync(int id, CancellationToken cancellation);



    }
}
