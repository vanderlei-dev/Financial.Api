# How to run

Download and install:

- [Docker](https://docs.docker.com/get-started/get-docker/)
- [.NET 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)

**Option 1 - Docker Compose in Visual Studio**

- In the root folder open the solution `Financial.Api.slnx` using [Visual Studio 2026](https://visualstudio.microsoft.com/en-us/vs/). 
- Make sure to select `Docker Compose` as the launch profile (right click on `docker-compose` project and select `Set as Startup Project`).
- Run the project (`F5` or `Debug > Start Debugging`).
- Use the Swagger UI in the opened browser to test the application.

**Option 2 - External Docker Compose**

Navigate to the folder `Financial.Api` and run the following command:

```
docker compose up -d
```

After the containers are up you should be able to access the Swagger UI to test the application on https://localhost:7201/swagger/index.html

To clean up the containers run:

```
docker compose down
```

## Running the unit tests

In the root folder run the following command:

```
dotnet test Financial.Api.slnx
```

# Architecture

The project follows a simplified folder organization approach, the main logic can be found in the `Endpoints` folder, which contains the implementations of the endpoints using minimal APIs.
The database choice was Postgres due to its flexibility, easy setup for containers and high performance.

### Endpoints:

- `POST companies/import` - Endpoint to import predefined companies from SEC's EDGAR API in the database. It uses the list of companies from the `appSettings.json` file and also erases the existing data in the database if present.
- `GET companies` - List the companies with their funding information optionally filtering by the starting letter.

# Remarks

- **Database Migrations** are executed automatically when the application starts for making the setup easier (in a real production app this should be replaced by migrations in a CI pipeline).
- **Unit Tests** were written with the help of GitHub Copilot, other parts of the code were **NOT** assisted by Copilot. The tests are focused on the listing of companies with the expected funding calculations.
- **By the time this project was developed, only 92/100 provided companies were returned from the SEC's EDGAR API.**