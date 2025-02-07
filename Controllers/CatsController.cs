using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Text.Json;

using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.EntityFrameworkCore;
//using StealAllTheCats.Data;
//using StealAllTheCats.Models;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

using System.Data; 
using StealAllTheCats.Models;
using StealAllTheCats.Data;


//namespace MyApp.Namespace
namespace StealAllTheCats.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatsController : ControllerBase
    {

        private readonly HttpClient _httpClient;
        //private readonly AppDbContext _context;
        private readonly ApplicationDbContext _context;

        private static readonly List<CatEntity> _cats = new(); // Temporary storage
        public CatsController(ApplicationDbContext context)
        {
            _httpClient = new HttpClient();
            _context = context;

        }
        
        

        //Testing(Fetch 25 Cats from Cat Server and return Data)   
        [HttpGet("fetch25Cats")]
        public async Task<IActionResult> Fetch25Cats()
        {
            return Ok("Stop Process");
            string apiUrl = "https://api.thecatapi.com/v1/images/search?limit=25"; // Fetch 25 cat images
            var response = await _httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Failed to fetch cats from TheCatAPI");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            return Ok(jsonResponse); // Returns raw JSON response from TheCatAPI
        }
    
        //First Endpoint(Post 25 cats in LocalServer after you get them from Cat Server)
        //

        //Second EndPoint(Get Cat By id)
        //GET /api/cats/{id}:
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCatById(int id)
        {
            var cat = await _context.Cats
                .FirstOrDefaultAsync(c=>c.Id==id);
            if(cat==null){
                return NotFound($"Cat with id:{id} not Found");
            }
            return Ok(cat); // Returns raw JSON response from TheCatAPI
        }
    
        //Third Endpoint(Retrieve Cats with paging support)
        //GET /api/cats?page=1&pageSize=10
        [HttpGet]
        public async Task<IActionResult> GetCats([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            // Check for valid page and pageSize values
            if (page < 1 || pageSize < 1)
            {
                return BadRequest("Page and pageSize must be greater than 0.");
            }

            // Get the total count of cats in the database
            var totalCats = await _context.Cats.CountAsync();

            // Retrieve cats with pagination (skip and take based on page and pageSize)
            var cats = await _context.Cats
                .Include(c => c.CatTags) // Include tags if needed
                .ThenInclude(ct => ct.TagEntity) // Include the related TagEntity for each tag
                .OrderBy(c => c.Id) // Sorting by ID for consistency
                .Skip((page - 1) * pageSize) // Skip previous pages
                .Take(pageSize) // Take only the number of cats specified by pageSize
                .ToListAsync();

            // Prepare the response with pagination information
            var response = new
            {
                TotalCats = totalCats,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCats / pageSize),
                Data = cats
            };

            return Ok(response); // Return the paginated response
        }

        //Fourth Endpoint(Retrieve cats with a specific tag and paging support)
        //GET /api/cats?tag=playful&page=1&pageSize=10
        [HttpGet("search")]
        public async Task<IActionResult> GetCatsByTag([FromQuery] string tag, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
            {
                return BadRequest("Page and pageSize must be greater than 0.");
            }

            var query = _context.Cats.AsQueryable();

            // Filter by tag if specified
            if (!string.IsNullOrEmpty(tag))
            {
                query = query.Where(c => c.CatTags.Any(ct => ct.TagEntity.Name == tag)); // Filter by tag
            }

            var totalCats = await query.CountAsync();

            var cats = await query
                .Include(c => c.CatTags)
                .ThenInclude(ct => ct.TagEntity) // Include the related TagEntity for each tag
                .OrderBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = new
            {
                TotalCats = totalCats,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCats / pageSize),
                Data = cats
            };

            return Ok(response); // Return the paginated response
        }
    }  

   
}
