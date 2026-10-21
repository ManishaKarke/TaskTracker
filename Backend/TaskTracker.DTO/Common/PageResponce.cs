using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskTracker.DTO.Common
{
    public class PageResponce<T>
    {
        public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
        public int Page {  get; set; }

        public int PageSize {  get; set; }
        public int PageCount { get; set; } 
        public int TotalCount { get; set; }
    }
}
