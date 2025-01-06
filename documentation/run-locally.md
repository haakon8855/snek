# Run project locally

This document will guide you through the process of setting up the local environment to run the project
locally.

## Setting up Application Config

In order to run the app locally two app-configs are required:

- Backend app-config located in: `src/Web/appsettings.Development.json` with the following contents:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "<database-connection-string>"
  }
}
```

- Frontend app-config located in: `src/Web.Client/wwwroot/appsettings.Development.json` with the following contents:

```json
{
  "Endpoints": {
    "ServerApi": "https://localhost:7274/"
  }
}
```

## Setting up a Local Database

To set up a database, you can use whatever tool of preference. Here we will use SQL Edge running in Docker.

1. Install Docker (Docker Desktop recommended for inexperienced users)
2. Pull and install the [SQL Edge](https://hub.docker.com/r/microsoft/azure-sql-edge) image in Docker. Follow the container's documentation on how to set it up.  
    - The documentation will most likely lead you to a command similar to this:  

      ```bash
      docker run --cap-add SYS_PTRACE -e 'ACCEPT_EULA=1' -e 'MSSQL_SA_PASSWORD=<your password>' -p 1433:1433 --name azuresqledge -d mcr.microsoft.com/azure-sql-edge
      ```  

      Create a password and choose a name for your container.  
        - The password requires at least 8 characters and needs to contain numbers, special characters and both upper and lowercase characters.
3. Insert your database connection string into your appsettings.Development.json-file.  
   If you used SQL Edge with Docker and the command above, your database should work with the following connection string:

   ```text
   Server=localhost,1433;Database=Snek;Trusted_Connection=false;User Id=SA;Password=<your-password>;Persist Security Info=False;encrypt=False
   ```

4. Create your database by applying the database migrations:

    ```bash
    cd src/Web
    dotnet ef database update
    ```

## Running the application

With both appsettings and the database set up, you can run the application by starting the Web-project.
