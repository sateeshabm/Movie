using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieAPIDemo.Data;
using MovieAPIDemo.Entities;
using MovieAPIDemo.Models;

namespace MovieAPIDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovieController : ControllerBase
    {
        private readonly MovieDbContext _context;
        private readonly IMapper _mapper;

        public MovieController(MovieDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Get(int pageIndex = 0, int pageSize = 10)
        {
            BaseResponseModel response = new BaseResponseModel();
            try
            {
                var movieCount = _context.Movie.Count();
                var movieList = _mapper.Map<List<MovieListViewModel>>(_context.Movie.Include(x => x.Actors).Skip(pageIndex * pageSize).Take(pageSize).ToList());

                response.Status = true;
                response.Message = "Success";
                response.Data = new { Movies = movieList, count = movieCount };

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong";

                return BadRequest(response);
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetMovieById(int id)
        {
            BaseResponseModel response = new BaseResponseModel();
            try
            {
                var movie = _context.Movie.Include(x => x.Actors).Where(x => x.Id == id).FirstOrDefault();

                if (movie == null)
                {
                    response.Status = false;
                    response.Message = "Movie not found";

                    return BadRequest(response);
                }

                var movieData = _mapper.Map<MovieDetailsViewModel>(movie);

                response.Status = true;
                response.Message = "Success";
                response.Data = movieData;

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong";

                return BadRequest(response);
            }
        }

        [HttpPost]
        public IActionResult Post(CreateMovieViewModel model)
        {
            BaseResponseModel response = new BaseResponseModel();

            try
            {
                if (ModelState.IsValid)
                {
                    var actors = _context.Person.Where(x => model.Actors.Contains(x.Id)).ToList();

                    if (actors.Count != model.Actors.Count)
                    {
                        response.Status = false;
                        response.Message = "Invalid Actors assigned";

                        return BadRequest(response);
                    }

                    // it's not good practice when you have multiple layers
                    var postedModel = _mapper.Map<Movie>(model);
                    postedModel.Actors = actors;

                    _context.Movie.Add(postedModel);
                    _context.SaveChanges(); //you can use async in this for larger application

                    var responseData = _mapper.Map<MovieDetailsViewModel>(postedModel);

                    response.Status = true;
                    response.Message = "Created Successfully.";
                    response.Data = responseData;

                    return Ok(response);
                }
                else
                {
                    response.Status = false;
                    response.Message = "Validation Failed";
                    response.Data = ModelState;

                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong";

                return BadRequest(response);
            }
        }

        [HttpPut]
        public IActionResult Put(CreateMovieViewModel model)
        {
            BaseResponseModel response = new BaseResponseModel();

            try
            {
                if (model.Id <= 0)
                {
                    response.Status = false;
                    response.Message = "Invalid movie record.";
                    return BadRequest(response);
                }

                var actors = _context.Person.Where(x => model.Actors.Contains(x.Id)).ToList();

                if (actors.Count != model.Actors.Count)
                {
                    response.Status = false;
                    response.Message = "Invalid actors assigned.";
                    return BadRequest(response);
                }

                var movieDetails = _context.Movie
                    .Include(x => x.Actors)
                    .FirstOrDefault(x => x.Id == model.Id);

                if (movieDetails == null)
                {
                    response.Status = false;
                    response.Message = "Invalid movie record.";
                    return BadRequest(response);
                }

                // Update movie details
                movieDetails.CoverImage = model.CoverImage;
                movieDetails.Description = model.Description;
                movieDetails.Language = model.Language;
                movieDetails.ReleaseDate = model.ReleaseDate;
                movieDetails.Title = model.Title;

                // Find and remove actors no longer assigned
                var removedActors = movieDetails.Actors
                    .Where(x => !model.Actors.Contains(x.Id))
                    .ToList();

                foreach (var actor in removedActors)
                {
                    movieDetails.Actors.Remove(actor);
                }

                // Find and add new actors
                var addedActors = actors
                    .Where(x => !movieDetails.Actors.Any(y => y.Id == x.Id))
                    .ToList();

                foreach (var actor in addedActors)
                {
                    movieDetails.Actors.Add(actor);
                }

                _context.SaveChanges();

                // Prepare response data
                var responseData = new MovieDetailsViewModel
                {
                    Id = movieDetails.Id,
                    Title = movieDetails.Title,
                    Actors = movieDetails.Actors.Select(y => new ActorViewModel
                    {
                        Id = y.Id,
                        Name = y.Name,
                        DateOfBirth = y.DateOfBirth,
                    }).ToList(),
                    CoverImage = movieDetails.CoverImage,
                    Language = movieDetails.Language,
                    ReleaseDate = movieDetails.ReleaseDate,
                    Description = movieDetails.Description,
                };

                response.Status = true;
                response.Message = "Updated successfully.";
                response.Data = responseData;

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                Console.WriteLine($"Error: {ex.Message}");

                response.Status = false;
                response.Message = "Something went wrong.";
                return BadRequest(response);
            }
        }


        [HttpDelete]
        public IActionResult Delete(int id)
        {
            BaseResponseModel response = new BaseResponseModel();

            try
            {
                var movie = _context.Movie.Where(x => x.Id == id).FirstOrDefault();
                if (movie == null)
                {
                    response.Status = false;
                    response.Message = "Invalid Movie Record";

                    return BadRequest(response);
                }

                _context.Movie.Remove(movie);
                _context.SaveChanges();

                response.Status = true;
                response.Message = "Deleted Successfully";

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong";

                return BadRequest(response);
            }
        }

        [HttpPost]
        [Route("upload-movie-poster")]
        public async Task<IActionResult> UploadMoviePoster(IFormFile imageFile)
        {
            try
            {
                // Validate file existence
                if (imageFile == null || imageFile.Length == 0)
                {
                    return BadRequest(new BaseResponseModel
                    {
                        Status = false,
                        Message = "No file uploaded."
                    });
                }

                // Get the file name and ensure it is clean of any extra quotes
                var filename = ContentDispositionHeaderValue.Parse(imageFile.ContentDisposition).FileName.TrimStart('\"').TrimEnd('\"');

                // Directory to store images (ensure it's outside the project directory for production use)
                string newPath = @"D:\Uploads"; // Change path to a desired location
                if (!Directory.Exists(newPath))
                {
                    Directory.CreateDirectory(newPath);
                }

                // Allowed file extensions for images
                string[] allowedImageExtensions = new string[] { ".jpg", ".jpeg", ".png" };

                // Check if file extension is allowed
                if (!allowedImageExtensions.Contains(Path.GetExtension(filename).ToLower()))
                {
                    return BadRequest(new BaseResponseModel
                    {
                        Status = false,
                        Message = "Only .jpg, .jpeg and .png type files are allowed."
                    });
                }

                // Generate a unique filename for the uploaded file
                string newFileName = Guid.NewGuid() + Path.GetExtension(filename);
                string fullFilePath = Path.Combine(newPath, newFileName);

                // Save the file to the disk
                using (var stream = new FileStream(fullFilePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                // Return the URL where the uploaded image can be accessed
                return Ok(new { ProfileImage = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/StaticFiles/{newFileName}" });
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                return BadRequest(new BaseResponseModel
                {
                    Status = false,
                    Message = "An error occurred while uploading the file: " + ex.Message
                });
            }
        }

    }
}