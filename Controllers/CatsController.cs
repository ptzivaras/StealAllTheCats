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
        
        

        // ✅ New endpoint: Fetch 25 cats from TheCatAPI and return the data
        [HttpGet("fetch")]
        public async Task<IActionResult> FetchCats()
        {
            string apiUrl = "https://api.thecatapi.com/v1/images/search?limit=25"; // Fetch 25 cat images
            var response = await _httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Failed to fetch cats from TheCatAPI");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            return Ok(jsonResponse); // Returns raw JSON response from TheCatAPI
        }
    
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
    
        [HttpGet]
        public async Task<IActionResult> GetCatById([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var totalCats = await _context.Cats.CountAsync();
            var cats = await _context.Cats
                .Include(c => c.CatTags) // Include tags if needed
                .ThenInclude(ct=>ct.CatEntity)
                .OrderBy(c => c.Id) // Sorting by ID for consistency
                .Skip((page - 1) * pageSize) // Skip previous pages
                .Take(pageSize) // Take only pageSize amount
                .ToListAsync();

                var response = new{
                    TotalCats = totalCats,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCats / pageSize),
                    Data = cats
                };
                return Ok(response);
        }
    
        // [HttpPost("fetch")]
        // public async Task<IActionResult> SaveUniqueCats()
        // {
        //     string apiUrl = "https://api.thecatapi.com/v1/images/search?limit=25";
        //     var response = await _httpClient.GetAsync(apiUrl);

        //     if (!response.IsSuccessStatusCode)
        //     {
        //        return StatusCode((int)response.StatusCode, "Failed to fetch cats from TheCatAPI");
        //     }

        //     var jsonResponse = await response.Content.ReadAsStringAsync();
        //     var fetchedCats = JsonSerializer.Deserialize<List<CatEntity>>(jsonResponse);

        //     if (fetchedCats == null || fetchedCats.Count == 0)
        //     {
        //         return BadRequest("No cats found.");
        //     }

        //     // 🚀 Save only unique cats (no duplicates)
        //     int beforeCount = _cats.Count;
        //     foreach (var cat in fetchedCats)
        //     {
        //         if (!_cats.Any(c => c.CatId == cat.CatId)) // Prevent duplicate CatId
        //         {
        //             _cats.Add(cat);
        //         }
        //     }

        //     int addedCount = _cats.Count - beforeCount;

        //     return Ok(new { message = "Cats saved successfully!", added = addedCount, total = _cats.Count });
        // }
        // ✅ Fetch and save unique cats into SQL Server
    
        // //Returns paginated data & total count & total pages
        // [HttpGet]
        // public async Task<IActionResult> GetCats([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        // {
        //     if (page < 1 || pageSize < 1)
        //     {
        //         return BadRequest(new { message = "Page and pageSize must be greater than 0." });
        //     }

        //     var totalCats = await _context.Cats.CountAsync();
        //     var cats = await _context.Cats
        //         .OrderBy(c => c.Id)
        //         .Skip((page - 1) * pageSize)
        //         .Take(pageSize)
        //         .ToListAsync();

        //     return Ok(new
        //     {
        //         totalCats,
        //         page,
        //         pageSize,
        //         totalPages = (int)Math.Ceiling((double)totalCats / pageSize),
        //         data = cats
        //     });
        // }

        // //specific tag & paging
        // [HttpGet("search")]
        // public async Task<IActionResult> GetCatsByTag([FromQuery] string tag, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        // {
        //     if (page < 1 || pageSize < 1)
        //     {
        //         return BadRequest(new { message = "Page and pageSize must be greater than 0." });
        //     }

        //     var query = _context.Cats.AsQueryable();

        //     if (!string.IsNullOrEmpty(tag))
        //     {
        //         query = query.Where(c => c.CatId.Contains(tag)); // Example: Adjust according to your tag structure
        //     }

        //     var totalCats = await query.CountAsync();
        //     var cats = await query
        //         .OrderBy(c => c.Id)
        //         .Skip((page - 1) * pageSize)
        //         .Take(pageSize)
        //         .ToListAsync();

        //     return Ok(new
        //     {
        //         totalCats,
        //         page,
        //         pageSize,
        //         totalPages = (int)Math.Ceiling((double)totalCats / pageSize),
        //         data = cats
        //     });
        // }

    }  

   
}
