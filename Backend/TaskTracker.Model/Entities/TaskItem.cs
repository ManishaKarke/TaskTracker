using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskTracker.Model.Enums;

namespace TaskTracker.Model.Entities
{
    public class TaskItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage ="Project is required")]
        public int ProjectId { get; set; }

        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string? Title { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        public Enums.TaskStatus Status { get; set; } = Enums.TaskStatus.Todo;

        public Enums.TaskPriority Priority { get; set; } = Enums.TaskPriority.Low;

        [StringLength(100, ErrorMessage = "AssignedTo cannot exceed 200 characters")]
        public string? AssignedTo { get; set; }

        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        [ForeignKey(nameof(ProjectId))]
        public Project Project { get; set; } = null;

    }
}
