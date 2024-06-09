using System;
using System.Collections.Generic;
using System.Text;

namespace Wesky.Net.OpenTools.HttpExtensions
{
    public class OpenToolResult<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public T Result { get; set; } 

    }
}
