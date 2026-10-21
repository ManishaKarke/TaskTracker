using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskTracker.DTO.Response
{
    public class ProjectResponseResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }

        public int TaskCount { get; set; }
        public string RowVersion { get; set; } =string.Empty;
    }
}
