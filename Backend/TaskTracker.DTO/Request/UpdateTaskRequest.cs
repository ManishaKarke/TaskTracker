using System.ComponentModel.DataAnnotations;
using TaskTracker.Model.Enums;
using TaskStatus = TaskTracker.Model.Enums.TaskStatus;

namespace TaskTracker.DTO.Request
{
    public class UpdateTaskRequest
    {
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string? Title { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        public TaskStatus Status { get; set; } = TaskStatus.Todo;

        public TaskPriority Priority { get; set; } = TaskPriority.Low;

        [StringLength(100, ErrorMessage = "AssignedTo cannot exceed 200 characters")]
        public string? AssignedTo { get; set; }

        public DateTime? DueDate { get; set; }

        public string RowVersion { get; set; } = string.Empty;
    }
}
