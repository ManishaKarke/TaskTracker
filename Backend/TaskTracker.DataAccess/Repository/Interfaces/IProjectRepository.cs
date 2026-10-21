using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskTracker.Model.Entities;

namespace TaskTracker.DataAccess.Repository.Interfaces
{
    public interface IProjectRepository
    {
        Task<List<ProjectSummary>> GetAllAsync(CancellationToken cancellationToken);

        Task<Project?> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<Project?> GetByNameAsync(string name, CancellationToken cancellationToken);
        Task<bool> ExistByNameAsync(string name, int? excludeProjectId, CancellationToken cancellationToken);

        Task AddAsync(Project project, CancellationToken cancellation);
        void Update(Project project);
        void Delete(Project project);
        Task<int> SaveChnagesAsync(CancellationToken cancellation);

        Task<Project?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken);
        void UpdateWithConcurrency(Project project, byte[] originalRowVersion);


    }
}
