using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovieAPIDemo.Models
{
    public class BaseResponseModel
    {
        public bool Status { get; set; }

        public string? Message { get; set; }
        public object? Data { get; set; }


    }

}