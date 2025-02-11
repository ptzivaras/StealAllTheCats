# StealAllTheCats
  This project is an ASP.NET Core Web API that interacts with the "Cats as a Service" (CaaS) API to fetch 25 cat images and store them in a Microsoft SQL Server database using Entity Framework Core. The application allows you to manage and retrieve cat images with their associated tags (temperament) and implement pagination and filtering by tags.

# Prerequisites
  .NET 8 SDK
  Microsoft SQL Server
  SQL Server Management Studio (SSMS) (optional, for database management)
  Postman or a similar tool for testing API requests
  Git (for cloning the repository)
  Api-Key for fetching 25 images (https://thecatapi.com/)

# Installation & Setup
  1. Clone the Repository
  git clone https://github.com/your-username/StealAllTheCats.git
  cd StealAllTheCats
  2. Configure the Database
  Open appsettings.json and update the database info and api-key
  Ensure your SQL Server is running and configured to accept connections.
  Apply database migrations using Entity Framework Core

# Running the Application
 dotnet run: This will start the server at https://localhost:5260
 Access Swagger API Documentation:  http://localhost:5260/swagger

# API Endpoints
Fetch 25 cats from TheCatAPI and save to the database.
Retrieve a cat by its database ID.
Retrieve paginated cat records.
Retrieve paginated cats with a specific tag.

# Technologies Used
ASP.NET Core 8.0 - Web API framework
Entity Framework Core - ORM for database management
Microsoft SQL Server - Database storage
Swashbuckle (Swagger) - API documentation

# Future Improvements
Implement Unit Tests
Add support for Docker Compose
Improve Error Handling & Logging
Break project in more parts based on Single-responsibility principle etc
Improve MVC (Repository is DBContext but maybe i will add Front End code and a new Repository and More Controllers and Services)

