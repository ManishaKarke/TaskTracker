using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskTracker.Services.Exceptions
{
    public sealed class ServiceExceptioncs : Exception
    {
        public int StatusCode { get; }
        public string ErrorCode { get; }

        public ServiceExceptioncs(int statusCode, string errorCode, string message) : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }
    }
}
