using Azure.Core;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using TaskTracker.DataAccess.Repository.Interfaces;
using TaskTracker.DTO.Request;
using TaskTracker.DTO.Response;
using TaskTracker.Model.Entities;
using TaskTracker.Services.Exceptions;
using TaskTracker.Services.Interfaces;

namespace TaskTracker.Services.Implementations
{
    public class ProjectService : IProjectServices
    {

        private readonly IProjectRepository _projectRepository;

        public ProjectService(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        async Task<ProjectResponseResponse> IProjectServices.CreateAsync(CreateProjectRequest createProjectRequest, CancellationToken cancellation)
        {
            ValidateProjectRequest(createProjectRequest.Name, createProjectRequest.Description);

            var name = createProjectRequest.Name.Trim();

            if (await _projectRepository.ExistByNameAsync(
                name, null, cancellation))
            {
                throw new ServiceExceptioncs(
                    409,
                    "duplicate_project_name",
                    "A project with this name already exists.");
            }

            var project = new Project
            {
                Name = name,
                Description = NormalizeOptionalText(createProjectRequest.Description)
            };

            try
            {
                await _projectRepository.AddAsync(
                    project, cancellation);

                await _projectRepository.SaveChnagesAsync(
                    cancellation);
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                throw new ServiceExceptioncs(
                    409,
                    "duplicate_project_name",
                    "A project with this name already exists.");
            }

            return MapToResponse(project, 0);


        }

        async Task<bool> IProjectServices.DeleteAsync(int id, CancellationToken cancellation)
        {
            if (id <= 0)
            {
                throw new ServiceExceptioncs(
                    400, "invalid_id", "Project ID must be greater than zero.");
            }

            var project = await _projectRepository.GetByIdForUpdateAsync(id, cancellation);

            if (project is null)
            {
                return false;
            }

            _projectRepository.Delete(project);
            await _projectRepository.SaveChnagesAsync(cancellation);

            return true;
        }

        async Task<IReadOnlyList<ProjectResponseResponse>> IProjectServices.GetAllAsync(CancellationToken cancellation)
        {
            var results = await _projectRepository.GetAllAsync(cancellation);

            return results.Select(result => MapToResponse(
                result.Project,
                result.TaskCount)).ToList();
        }

        async Task<ProjectResponseResponse> IProjectServices.GetByIdAsync(int id, CancellationToken cancellation)
        {
            if (id <= 0)
            {
                throw new ServiceExceptioncs(400, "invalid_id", "Project ID must be greater than zero.");
            }

            var project = await _projectRepository.GetByIdAsync(id,cancellation);

            return project is null ? null : MapToResponse(project, 0);
        }

        async Task<ProjectResponseResponse?> IProjectServices.UpdateAsync(int id, UpdateProjectRequest updateProjectRequest, CancellationToken cancellation)
        {
            if (id <= 0)
            {
                throw new ServiceExceptioncs(
                    400, "invalid_id", "Project ID must be greater than zero.");
            }

            ValidateProjectRequest(updateProjectRequest.Name, updateProjectRequest.Description);

            var clientRowVersion = DecodeRowVersion(updateProjectRequest.RowVersion);

            var project = await _projectRepository.GetByIdAsync(
                id, cancellation);

            if (project is null)
            {
                return null;
            }

            EnsureRowVersionLength(clientRowVersion);
            var duplicateName = await _projectRepository.ExistByNameAsync(
               updateProjectRequest.Name.Trim(), id, cancellation);

            if (duplicateName)
            {
                throw new ServiceExceptioncs(
                    409,
                    "duplicate_project_name",
                    "Another project already uses this name.");
            }

            project.Name = updateProjectRequest.Name.Trim();
            project.Description = NormalizeOptionalText(updateProjectRequest.Description);

            _projectRepository.UpdateWithConcurrency(project, clientRowVersion);

            try
            {
                await _projectRepository.SaveChnagesAsync(cancellation);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ServiceExceptioncs(
                    409,
                    "concurrency_conflict",
                    "The project was modified by another user. Reload it and try again.");
            }

            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                throw new ServiceExceptioncs(
                    409,
                    "duplicate_project_name",
                    "Another project already uses this name.");
            }

            return MapToResponse(project, 0);

        }

        private static ProjectResponseResponse MapToResponse(Project project,int taskCount)
        {
            return new ProjectResponseResponse
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                CreatedAt = project.CreatedAt,
                TaskCount = taskCount,
                RowVersion = Convert.ToBase64String(project.RowVersion)
            };
        }

        private static void ValidateProjectRequest(string? name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ServiceExceptioncs(
                    400, "validation_error", "Project name is required.");
            }

            if (name.Trim().Length > 100)
            {
                throw new ServiceExceptioncs(
                    400, "validation_error",
                    "Project name cannot exceed 100 characters.");
            }

            if (description?.Length > 500)
            {
                throw new ServiceExceptioncs(
                    400, "validation_error",
                    "Project description cannot exceed 500 characters.");
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
                return Convert.FromBase64String(value);
            }
            catch (FormatException)
            {
                throw new ServiceExceptioncs(
                    400, "invalid_row_version",
                    "RowVersion must be a valid Base64 string.");
            }
        }

        private static void EnsureRowVersionLength(byte[] rowVersion)
        {
            if (rowVersion.Length != 8)
            {
                throw new ServiceExceptioncs(
                    400, "invalid_row_version",
                    "RowVersion must contain a valid SQL Server row version.");
            }
        }

        private static bool IsUniqueConstraintViolation(Exception exception)
        {
            for (Exception? current = exception;
                 current is not null;
                 current = current.InnerException)
            {
                if (current is SqlException sqlException &&
                    (sqlException.Number == 2601 ||
                     sqlException.Number == 2627))
                {
                    return true;
                }
            }

            return false;
        }

    }
}
