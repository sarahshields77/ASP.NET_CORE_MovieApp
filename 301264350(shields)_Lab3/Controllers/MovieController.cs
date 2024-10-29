/*
 * Student Name: Sarah Shields
 * Student Number: 301264350
 * Submission Date: October 30th, 2024
 */

using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using _301264350_shields__Lab3.Models;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.S3;
using System.Security.Claims;

namespace _301264350_shields__Lab3.Controllers
{
    public class MovieController : Controller
    {
        private readonly IDynamoDBContext _dynamoDbContext;
        private readonly IAmazonS3 _s3Client;

        public MovieController(IDynamoDBContext dynamoDbContext, IAmazonS3 s3Client)
        {
            _dynamoDbContext = dynamoDbContext;
            _s3Client = s3Client;
        }

        // GET: show dashboard with filtered movies
        [HttpGet]
        public async Task<IActionResult> Dashboard(string genre, double? rating)
        {
            // build scan conditions based on provided filters
            var scanConditions = new List<ScanCondition>();

            if (!string.IsNullOrEmpty(genre))
            {
                scanConditions.Add(new ScanCondition("Genre", ScanOperator.Equal, genre));
            }

            if (rating.HasValue)
            {
                scanConditions.Add(new ScanCondition("Rating", ScanOperator.GreaterThanOrEqual, rating.Value));
            }

            // retrieve filtered movies from DynamoDB
            var movies = await _dynamoDbContext.ScanAsync<Movie>(scanConditions).GetRemainingAsync();
            return View(movies);
        }

        // GET: add movie form
        [HttpGet]
        public IActionResult AddMovie()
        {
            return View();
        }
        // POST: add movie action
        [HttpPost]
        public async Task<IActionResult> AddMovie(Movie movie, IFormFile movieFile)
        {
            Console.WriteLine("Form submitted to AddMovie action");

            if (movieFile == null || movieFile.Length == 0)
            {
                Console.WriteLine("No movie file selected or file is empty");
                ModelState.AddModelError("movieFile", "Please upload a movie file.");
            }

            // remove ModelState validation errors for fields set in the controller
            ModelState.Remove("S3Url");
            ModelState.Remove("MovieId");
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                movie.MovieId = Guid.NewGuid().ToString();
                Console.WriteLine($"Generated MovieId: {movie.MovieId}");

                // set UserId to the logged-in user's ID
                movie.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                Console.WriteLine($"Set UserId to: {movie.UserId}");

                // upload movie file to S3
                var s3Url = await UploadMovieToS3Async(movieFile, movie.MovieId);
                movie.S3Url = s3Url;
                Console.WriteLine($"Set S3Url to: {movie.S3Url}");

                // initialize rating values
                movie.Rating = 0;
                movie.NumberOfRatings = 0;

                // save to DynamoDB
                await _dynamoDbContext.SaveAsync(movie);
                Console.WriteLine("Movie saved to DynamoDB");

                return RedirectToAction("Dashboard");
            }

            // log validation errors if the model state is invalid
            Console.WriteLine("ModelState is invalid");
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }

            return View(movie);
        }

        // GET: details page for a movie
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            // Retrieve the movie by ID
            var movie = await _dynamoDbContext.LoadAsync<Movie>(id);

            // Retrieve comments associated with the movie
            var comments = await _dynamoDbContext.ScanAsync<Comment>(
                new List<ScanCondition> { new ScanCondition("MovieId", ScanOperator.Equal, id) }
            ).GetRemainingAsync();

            var viewModel = new MovieDetailsViewModel
            {
                Movie = movie,
                Comments = comments
            };

            return View(viewModel);
        }

        // GET: edit movie form
        [HttpGet]
        public async Task<IActionResult> EditMovie(string id)
        {
            var movie = await _dynamoDbContext.LoadAsync<Movie>(id);

            if (movie != null && movie.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return View(movie);
            }

            return RedirectToAction("Details", new { id });
        }
        // POST: edit movie action
        [HttpPost]
        public async Task<IActionResult> EditMovie(Movie updatedMovie)
        {
            var movie = await _dynamoDbContext.LoadAsync<Movie>(updatedMovie.MovieId);

            if (movie != null && movie.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                // update movie details
                movie.Title = updatedMovie.Title;
                movie.Genre = updatedMovie.Genre;
                movie.Director = updatedMovie.Director;
                movie.ReleaseDate = updatedMovie.ReleaseDate;

                await _dynamoDbContext.SaveAsync(movie);
            }

            return RedirectToAction("Details", new { id = updatedMovie.MovieId });
        }

        // POST: add comment/rating action
        [HttpPost]
        public async Task<IActionResult> AddComment(string movieId, string commentText, double rating)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // create new comment with the provided rating
            var comment = new Comment
            {
                CommentId = Guid.NewGuid().ToString(),
                MovieId = movieId,
                UserId = userId,
                CommentText = commentText,
                Rating = rating,
                PostedAt = DateTime.UtcNow,
                Editable = true // initially set to true
            };

            await _dynamoDbContext.SaveAsync(comment);

            // retrieve movie to update rating information
            var movie = await _dynamoDbContext.LoadAsync<Movie>(movieId);
            if (movie != null)
            {
                // update moving rating information
                movie.Rating = (movie.Rating * movie.NumberOfRatings + rating) / (movie.NumberOfRatings + 1);
                movie.NumberOfRatings += 1;

                // save updated movie
                await _dynamoDbContext.SaveAsync(movie);
            }

            return RedirectToAction("Details", new { id = movieId });
        }

        // GET: edit comment form
        [HttpGet]
        public async Task<IActionResult> EditComment(string id)
        {
            var comment = await _dynamoDbContext.LoadAsync<Comment>(id);

            // check if comment is editable
            if ((DateTime.UtcNow - comment.PostedAt).TotalHours > 24)
            {
                comment.Editable = false;
                await _dynamoDbContext.SaveAsync(comment);
                return RedirectToAction("Details", new { id = comment.MovieId });
            }

            return View(comment);
        }

        // POST: edit comment/rating action
        [HttpPost]
        public async Task<IActionResult> EditComment(string CommentId, string CommentText, double Rating)
        {
            // load comment to ensure it's editable
            var comment = await _dynamoDbContext.LoadAsync<Comment>(CommentId);

            if (comment.Editable && (DateTime.UtcNow - comment.PostedAt).TotalHours <= 24)
            {
                // update comment text and rating
                comment.CommentText = CommentText;
                comment.Rating = Rating;

                await _dynamoDbContext.SaveAsync(comment);
            }

            return RedirectToAction("Details", new { id = comment.MovieId });
        }

        // upload movie file to S3
        private async Task<string> UploadMovieToS3Async(IFormFile movieFile, string movieId)
        {
            var bucketName = "moviesbucket-assign3"; // Replace with your actual bucket name
            var key = $"movies/{movieId}/{movieFile.FileName}";

            using var stream = movieFile.OpenReadStream();

            var request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = key,
                InputStream = stream,
                ContentType = movieFile.ContentType
            };

            var response = await _s3Client.PutObjectAsync(request);

            // Return the public URL of the file in the S3 bucket
            return $"https://{bucketName}.s3.amazonaws.com/{key}";
        }

        // download movie file from S3
        public async Task<IActionResult> DownloadMovie(string id)
        {
            var movie = await _dynamoDbContext.LoadAsync<Movie>(id);

            if (movie == null) return RedirectToAction("Details", new { id });

            var request = new GetObjectRequest
            {
                BucketName = "moviesbucket-assign3",
                Key = $"movies/{movie.MovieId}/{movie.Title}.mp4"
            };

            var response = await _s3Client.GetObjectAsync(request);
            return File(response.ResponseStream, "video/mp4", $"{movie.Title}.mp4");
        }

        // POST: delete movie action
        [HttpPost]
        public async Task<IActionResult> DeleteMovie(string id)
        {
            var movie = await _dynamoDbContext.LoadAsync<Movie>(id);

            if (movie != null && movie.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                // delete movie file from S3
                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = "moviesbucket-assign3",
                    Key = $"movies/{movie.MovieId}/{movie.Title}.mp4"
                };
                await _s3Client.DeleteObjectAsync(deleteRequest);

                // delete movie from DynamoDB
                await _dynamoDbContext.DeleteAsync<Movie>(id);
            }

            return RedirectToAction("Dashboard");
        }
    }
}
