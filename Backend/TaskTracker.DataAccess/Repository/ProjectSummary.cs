using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskTracker.Model.Entities;

namespace TaskTracker.DataAccess.Repository
{
    public sealed class ProjectSummary
    {
        public required Project Project { get; init; }
        public int TaskCount { get; init; }
    }
}
