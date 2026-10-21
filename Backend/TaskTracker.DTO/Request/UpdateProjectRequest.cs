using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskTracker.DTO.Request
{
    public class UpdateProjectRequest
    {
        [Required(ErrorMessage = "Project Name is required")]
        [StringLength(100, ErrorMessage = "Project name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description name cannot exceed 100 characters")]
        public string? Description { get; set; }

        public string RowVersion { get; set; } = string.Empty;
    }
}
