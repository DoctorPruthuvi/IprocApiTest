using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer("Server=LAPTOP-H0PTKPTI;Database=TaveMaze;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False"));
builder.Services.AddScoped<IShowRepository, ShowRepository>();
builder.Services.AddScoped<IShowService, ShowService>();
builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Show> Shows { get; set; }
}

public class Show
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Language { get; set; }
    public DateTime Premiered { get; set; }
    public List<string> Genres { get; set; }
    public string Summary { get; set; }
}


public interface IShowRepository
{
    Task<List<Show>> GetAllShowsAsync();
    Task<Show> GetShowByIdAsync(int id);
    Task AddShowAsync(Show show);
    Task UpdateShowAsync(Show show);
    Task DeleteShowAsync(int id);
}

public class ShowRepository : IShowRepository
{
    private readonly AppDbContext _context;
    public ShowRepository(AppDbContext context) { _context = context; }

    public async Task<List<Show>> GetAllShowsAsync() => await _context.Shows.ToListAsync();
    public async Task<Show> GetShowByIdAsync(int id) => await _context.Shows.FindAsync(id);


    public async Task AddShowAsync(Show show) { _context.Shows.Add(show); await _context.SaveChangesAsync(); }


    public async Task UpdateShowAsync(Show show) { _context.Shows.Update(show); await _context.SaveChangesAsync(); }

    public async Task DeleteShowAsync(int id) { var show = await _context.Shows.FindAsync(id); if (show != null) { _context.Shows.Remove(show); await _context.SaveChangesAsync(); } }
}

public interface IShowService
{
    Task<List<Show>> FetchAndStoreShowsAsync();

}

public class ShowService : IShowService
{
    private readonly HttpClient _httpClient;
    private readonly IShowRepository _showRepository;
    public ShowService(HttpClient httpClient, IShowRepository showRepository)
    {
        _httpClient = httpClient;
        _showRepository = showRepository;
    }

    public async Task<List<Show>> FetchAndStoreShowsAsync()
    {
        var response = await _httpClient.GetStringAsync("https://api.tvmaze.com/shows");
        //var shows = JsonSerializer.Deserialize<List<Show>>(response);
        var shows = JsonConvert.DeserializeObject<List<Show>>(response);
        foreach (var show in shows)
        {
            show.Id = 0;
            if (show.Premiered > new DateTime(2014, 1, 1))
            {
                await _showRepository.AddShowAsync(show);
            }
        }

        return await _showRepository.GetAllShowsAsync();
    }
}

[ApiController]
[Route("api/shows")]
public class ShowController : ControllerBase
{
    private readonly IShowService _showService;
    private readonly IShowRepository _showRepository;
    public ShowController(IShowService showService, IShowRepository showRepository)
    {
        _showService = showService;
        _showRepository = showRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllShows() => Ok(await _showRepository.GetAllShowsAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetShow(int id)
    {
        var show = await _showRepository.GetShowByIdAsync(id);
        return show == null ? NotFound() : Ok(show);
    }

    [HttpPost]
    public async Task<IActionResult> AddShow([FromBody] Show show)
    {
        await _showRepository.AddShowAsync(show);
        return CreatedAtAction(nameof(GetShow), new { id = show.Id }, show);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateShow(int id, [FromBody] Show show)
    {
        if (id != show.Id) return BadRequest();
        await _showRepository.UpdateShowAsync(show);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteShow(int id)
    {
        await _showRepository.DeleteShowAsync(id);
        return NoContent();
    }





    [HttpGet("fetch")]
    public async Task<IActionResult> FetchAndStoreShows() => Ok(await _showService.FetchAndStoreShowsAsync());
}
