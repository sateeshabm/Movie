using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MovieAPIDemo.Models
{
    public class MovieListViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<ActorViewModel> Actors { get; set; }
        public string Language { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string CoverImage { get; set; }
    }
}