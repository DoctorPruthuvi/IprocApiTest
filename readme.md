# Tavemaze Data Retrieval API


## Getting Started



### 1. Configure Database Connection

Modify `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=your_server;Database=TvMazeDB;Integrated Security=True;"
}
```


### 2. Apply Database Migrations

Run the following command to create the database
The initial create is in the code.

```bash
dotnet ef database update
```

### 3. Run the API


```bash
dotnet run
```


