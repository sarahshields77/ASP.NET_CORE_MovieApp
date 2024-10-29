namespace _301264350_shields__Lab3.Models
{
    public class MovieDetailsViewModel
    {
        public Movie Movie { get; set; }
        public IEnumerable<Comment> Comments { get; set; }
        public double AverageRating => Movie.NumberOfRatings > 0 ? Movie.Rating : 0;
    }   
}
