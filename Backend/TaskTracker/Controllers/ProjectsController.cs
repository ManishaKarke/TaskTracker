using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TaskTracker.DTO.Request;
using TaskTracker.Services.Interfaces;


namespace TaskTracker.Controllers
{
    [Route("api/projects")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectServices _projectServices;

        public ProjectsController(IProjectServices project)
        {
            _projectServices = project;
        }

        /// <summary>
        /// Retrive all projects with their task counts
        /// </summary>
        /// GET: api/projects
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken cancellation)
        {
            var projects = await _projectServices.GetAllAsync(cancellation);
            return Ok(projects);
        }

        /// <summary>
        ///  Retrieves a project by ID 
        /// </summary>
        /// GET: /api/projects/{id}
        [HttpGet("{id:int}", Name = "GetProjectById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellation)
        {
            var projects = await _projectServices.GetByIdAsync(id, cancellation);
            if(projects is null)
                return NotFound( new { message = $"Project with ID {id} was not found."});

            return Ok(projects);
        }

        /// <summary>
        /// Create a new project
        /// </summary>
        /// POST: /api/projects
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CreateProjectRequest request, CancellationToken cancellation)
        {
            var project = await _projectServices.CreateAsync(request, cancellation);

            return CreatedAtRoute("GetProjectById", new { id = project.Id},project);
        }

        /// <summary>
        ///  Update an existing project
        /// </summary>
        /// PUT: /api/projects/{id}
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProjectRequest request, CancellationToken cancellationToken)
        {
            var project = await _projectServices.UpdateAsync(id, request, cancellationToken);
            
            if(project is null)
            {
                return NotFound(new { message =$"Project with ID {id} was not found."});
            }
            
            return Ok(project);
        }

      /// <summary>
      /// Deletes a project and its related tasks
      /// </summary>
      /// DELETE : /api/projects/{id}
      [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var deleted = await _projectServices.DeleteAsync(id, cancellationToken);

            if (!deleted)
            {
                return NotFound(new { message = $"Project with ID {id} was not found." });
            }

            return NoContent();
        }
    }
}
