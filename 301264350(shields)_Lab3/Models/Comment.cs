using Amazon.DynamoDBv2.DataModel;

namespace _301264350_shields__Lab3.Models
{
    [DynamoDBTable("Comments")]
    public class Comment
    {
        [DynamoDBHashKey]
        public string CommentId { get; set; } 

        [DynamoDBProperty]
        public string MovieId { get; set; } 

        [DynamoDBProperty]
        public string UserId { get; set; } 

        [DynamoDBProperty]
        public string CommentText { get; set; }

        [DynamoDBProperty]
        public DateTime PostedAt { get; set; }

        [DynamoDBProperty]
        public double Rating { get; set; } 

        [DynamoDBProperty]
        public bool Editable { get; set; } 

    }
}
