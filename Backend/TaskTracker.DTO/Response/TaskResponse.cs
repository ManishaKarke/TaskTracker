using TaskTracker.Model.Enums;
using TaskStatus = TaskTracker.Model.Enums.TaskStatus;

namespace TaskTracker.DTO.Response
{
    public class TaskResponse
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public TaskStatus Status { get; set; }
        public TaskPriority priority { get; set; }

        public string? AssignedTo { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public string Rowversion { get; set; } = string.Empty;

    }
}
