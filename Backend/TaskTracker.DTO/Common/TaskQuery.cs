
using TaskTracker.Model.Enums;
using TaskStatus = TaskTracker.Model.Enums.TaskStatus;

namespace TaskTracker.DTO.Common
{
    public  class TaskQuery
    {
        public TaskStatus? Status { get; set; }
        public TaskPriority? Priority { get; set; }
        public string? Search {  get; set; }

        public string SortBy { get; set; } = "createdAt";

        public string SortDir { get; set; } = "desc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

    }
}
