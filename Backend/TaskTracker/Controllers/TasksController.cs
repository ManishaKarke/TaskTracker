using Microsoft.AspNetCore.Mvc;
using TaskTracker.DTO.Common;
using TaskTracker.DTO.Request;
using TaskTracker.Services.Interfaces;


namespace TaskTracker.Controllers
{
    [Route("api")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }


        /// <summary>
        /// Retrieves tasks for a projects with filtering, sorting and paging
        /// </summary>
        /// GET: /api/projects/{projectId}/tasks
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPaged(int projectId, [FromQuery] TaskQuery query, CancellationToken cancellationToken)
        {
            var result = await _taskService.GetPageAsync(projectId, query, cancellationToken);
            return Ok(result);
        }


        /// <summary>
        /// Retrieves one task by ID.
        /// GET: /api/tasks/{id}
        /// </summary>
        [HttpGet("tasks/{id:int}", Name = "GetTaskById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var task = await _taskService.GetByIdAsyc(id, cancellationToken);

            if (task is null)
            {
                return NotFound(new
                {
                    message = $"Task with ID {id} was not found."
                });
            }

            return Ok(task);
        }

        /// <summary>
        /// Creates a task under the specified project.
        /// POST: /api/projects/{projectId}/tasks
        /// </summary>
        [HttpPost("projects/{projectId:int}/tasks")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create(int projectId, [FromBody] CreateTaskRequest request, CancellationToken cancellationToken)
        {
            var task = await _taskService.CreateAsync(
                projectId, request, cancellationToken);

            return CreatedAtRoute( "GetTaskById",  new { id = task.Id },  task);
        }

        /// <summary>
        /// Updates an existing task using optimistic concurrency.
        /// PUT: /api/tasks/{id}
        /// </summary>
        [HttpPut("tasks/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskRequest request, CancellationToken cancellationToken)
        {
            var task = await _taskService.UpdateAsync(id, request, cancellationToken);

            if (task is null)
            {
                return NotFound(new
                {
                    message = $"Task with ID {id} was not found."
                });
            }

            return Ok(task);
        }

        /// <summary>
        /// Changes only the status of a task.
        /// PATCH: /api/tasks/{id}/status
        /// </summary>
        [HttpPatch("tasks/{id:int}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateTaskStatusRequest request, CancellationToken cancellationToken)
        {
            var task = await _taskService.UpdateStatusAsync(id, request, cancellationToken);

            if (task is null)
            {
                return NotFound(new
                {
                    message = $"Task with ID {id} was not found."
                });
            }

            return Ok(task);
        }

        /// <summary>
        /// Deletes a task by ID.
        /// DELETE: /api/tasks/{id}
        /// </summary>
        [HttpDelete("tasks/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var deleted = await _taskService.DeleteAsync(id, cancellationToken);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = $"Task with ID {id} was not found."
                });
            }

            return NoContent();
        }
    }
}
