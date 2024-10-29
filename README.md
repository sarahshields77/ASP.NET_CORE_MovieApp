# Nextflix

Nextflix is an ASP.NET Core web application designed to compete with streaming giants. The platform allows registered users to upload, delete, and download movies, as well as interact with movie comments and ratings. The project leverages AWS services like Elastic Beanstalk, RDS, DynamoDB, and S3 for a scalable, cloud-based solution.

## Features

- **User Registration and Login**: User details are securely stored in an RDS database.
- **Movie Management**:
  - Upload and delete movies (uploaded videos stored in an S3 bucket).
  - Download movies directly from S3.
  - View movie details with metadata (title, genre, director, release date, etc.).
- **Comments and Ratings**:
  - Add comments to specific movies.
  - Rate movies, with automatic calculation of average rating.
  - Edit comments within 24 hours.
- **Filtering and Searching**:
  - Filter movies by genre and rating on the dashboard.

## Tech Stack

- **Frontend**: ASP.NET Core MVC with Razor views and Bootstrap for styling.
- **Backend**: ASP.NET Core, Entity Framework Core, and DynamoDB.
- **Cloud Infrastructure**: 
  - **AWS Elastic Beanstalk**: Application hosting and deployment.
  - **AWS RDS**: Stores user registration data.
  - **AWS DynamoDB**: Stores movie metadata and comments.
  - **AWS S3**: Manages video storage and downloading.

## Getting Started

To set up the project locally, ensure you have the following installed:

- [.NET SDK](https://dotnet.microsoft.com/download) (version 6 or later)
- [AWS CLI](https://aws.amazon.com/cli/)
- [MySQL Workbench](https://www.mysql.com/products/workbench/) or any SQL Client compatible with RDS

### Setup Steps

1. Clone the repository:

   ```bash
   git clone https://github.com/username/Nextflix.git
   cd Nextflix
