using TaskStatus = TaskTracker.Model.Enums.TaskStatus;

namespace TaskTracker.DTO.Request
{
    public class UpdateTaskStatusRequest
    {
        public TaskStatus Status { get; set; }
        public string RowVersion { get; set; } = string.Empty;
    }
}
