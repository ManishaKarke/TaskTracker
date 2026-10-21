using Microsoft.EntityFrameworkCore;
using TaskTracker.DataAccess.Context;
using TaskTracker.DataAccess.Repository.Interfaces;
using TaskTracker.Model.Entities;

namespace TaskTracker.DataAccess.Repository.Implementation
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly ApplicationDbContext _context;

        public ProjectRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Project project, CancellationToken cancellation)
        {
            await _context.Projects.AddAsync(project, cancellation);
        }

        public void Delete(Project project)
        {
            _context.Projects.Remove(project);
        }

        public async Task<bool> ExistByNameAsync(string name, int? excludeProjectId, CancellationToken cancellationToken)
        {
            var query =  _context.Projects.AsNoTracking().Where(project => project.Name == name);
            if(excludeProjectId.HasValue)
            {
                query = query.Where(Project => Project.Id != excludeProjectId.Value);
            }

            return await query.AnyAsync(cancellationToken);
           
        }

        public async Task<List<ProjectSummary>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _context.Projects.AsNoTracking().Select(Project => new ProjectSummary { Project = Project, TaskCount = Project.TaskItems.Count() }).OrderBy(Result => Result.Project.Name).ToListAsync(cancellationToken); 
        }

        public async Task<Project?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _context.Projects.AsNoTracking().FirstOrDefaultAsync(Project => Project.Id == id, cancellationToken);
        }

        public async Task<Project?> GetByNameAsync(string name, CancellationToken cancellationToken)
        {
            return await _context.Projects.AsNoTracking().FirstOrDefaultAsync(Project => Project.Name == name, cancellationToken);
        }

        public async Task<int> SaveChnagesAsync(CancellationToken cancellation)
        {
            return await _context.SaveChangesAsync(cancellation);
        }

        public void Update(Project project)
        {
            _context.Projects.Update(project);
        }
    }
}
