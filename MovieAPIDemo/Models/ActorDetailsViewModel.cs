using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovieAPIDemo.Models
{
    public class ActorDetailsViewModel : ActorViewModel
    {
        public string[] Movies { get; set; }
    }
}