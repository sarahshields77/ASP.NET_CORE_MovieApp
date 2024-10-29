using Amazon.DynamoDBv2.DataModel;

namespace _301264350_shields__Lab3.Models
{
    [DynamoDBTable("Movies")]
    public class Movie
    {
        [DynamoDBHashKey] // Primary Key (Partition Key)       
        public string MovieId { get; set; } // GUID generated in controller

        [DynamoDBProperty]
        public string Title { get; set; }

        [DynamoDBProperty]
        public string Genre { get; set; }

        [DynamoDBProperty]
        public string Director { get; set; }

        [DynamoDBProperty]
        public DateTime ReleaseDate { get; set; }

        [DynamoDBProperty]       
        public string S3Url { get; set; } // URL to the movie in S3 populated after upload

        [DynamoDBProperty]
        public double Rating { get; set; } // average rating

        [DynamoDBProperty]
        public int NumberOfRatings { get; set; } // number of ratings for average calculation

        [DynamoDBProperty]
        public string UserId { get; set; } // user who uploaded the movie
    }
}
