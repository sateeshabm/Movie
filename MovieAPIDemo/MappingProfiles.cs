using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MovieAPIDemo.Entities;
using MovieAPIDemo.Models;

namespace MovieAPIDemo
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Movie, MovieListViewModel>();
            CreateMap<Movie, MovieDetailsViewModel>();
            CreateMap<MovieListViewModel, Movie>();
            CreateMap<CreateMovieViewModel, Movie>().ForMember(x => x.Actors, y => y.Ignore()); //we are adding actor name as manually

            CreateMap<Person, ActorViewModel>();
            CreateMap<Person, ActorDetailsViewModel>();
            CreateMap<ActorViewModel, Person>();

        }
    }
}