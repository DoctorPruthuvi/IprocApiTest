# Tavemaze Data Retrieval API


## Getting Started



### 1. Configure Database Connection



```json

 options.UseSqlServer("Server=.;Database=TvMazeDB;Trusted_Connection=True;"));

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


