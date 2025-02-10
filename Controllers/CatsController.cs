using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using System.Data; 
using StealAllTheCats.Models;
using StealAllTheCats.Data;
using System.ComponentModel.DataAnnotations;
using StealAllTheCats.Dtos;
using StealAllTheCats.Services;

namespace StealAllTheCats.Controllers
{
    /// <summary>
    /// API Controller for Managing Cats
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CatsController : ControllerBase
    {

        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _context;
        private readonly CatService _catService;
        private readonly IConfiguration _configuration;

        private static readonly List<CatEntity> _cats = new(); // Temporary storage
        public CatsController(ApplicationDbContext context, CatService catService, IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _context = context;
            _catService = catService; // ✅ Assign injected service
            
            _configuration = configuration;
        }
 

        /// <summary>
        /// Fetches 25 cat images from TheCatAPI and saves them in the database.
        /// </summary>
        /// <returns>Returns a success message if cats are added.</returns>
        /// <response code="200">Successfully added cats.</response>
        /// <response code="400">Bad request (invalid input or validation failure).</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost("fetch")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> SaveUniqueCats([FromServices] CatService catService)
        {
            Console.WriteLine("[DEBUG] Fetch function called!");

            //string apiKey = "live_r28aR40CbiGE2ucr7fiQVsNiCfACtX0VopUMAMWk1YCxiUxOCgIB06gUcsr3vwrN";
            var apiKey = _configuration["CatApi:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                //_logger.LogError("API Key is missing from configuration.");
                return StatusCode(500, "Server configuration error: API key missing.");
            }
            string apiUrl = $"https://api.thecatapi.com/v1/images/search?limit=25&api_key={apiKey}";


            var response = await _httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("[DEBUG] Failed to fetch cats from API");
                return StatusCode((int)response.StatusCode, "Failed to fetch cats from TheCatAPI");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var fetchedCats = JsonSerializer.Deserialize<List<CatApiResponse>>(jsonResponse);

            if (fetchedCats == null || fetchedCats.Count == 0)
            {
                Console.WriteLine("[DEBUG] No cats found in API response");
                return BadRequest("No cats found.");
            }

            Console.WriteLine($"[DEBUG] Retrieved {fetchedCats.Count} cats from API");

            var newCats = new List<CatEntity>();
    

            foreach (var cat in fetchedCats)
            {
                if (cat.breeds == null || cat.breeds.Count == 0 || string.IsNullOrEmpty(cat.breeds[0].temperament))
                    continue;

                var newCat = new CatEntity
                {
                    CatId = cat.id,
                    Width = cat.width,
                    Height = cat.height,
                    ImageUrl = cat.url,
                    Created = DateTime.UtcNow,
                    CatTags = new List<CatTag>()
                };

                var validationResults = new List<ValidationResult>();
                var isValid = Validator.TryValidateObject(newCat, new ValidationContext(newCat), validationResults, true);

                if (!isValid)
                {
                    var errorMessages = validationResults.Select(vr => vr.ErrorMessage).ToList();
                    return BadRequest(new { Errors = errorMessages });
                }

                var tagNames = cat.breeds[0].temperament.Split(',')
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrEmpty(t))
                    .ToList();

                foreach (var tagName in tagNames)
                {
                    var tag = new TagEntity { Name = tagName, Created = DateTime.UtcNow };
                    newCat.CatTags.Add(new CatTag { TagEntity = tag });
                }

                newCats.Add(newCat);
            }

            //Check if `SaveCatsToDatabaseAsync` is being called
            Console.WriteLine($"[DEBUG] Sending {newCats.Count} new cats to service...");

            int catsSaved = await _catService.SaveCatsToDatabaseAsync(newCats);

            Console.WriteLine($"[DEBUG] Service returned {catsSaved} saved cats");

            return Ok(catsSaved > 0 ? $"Successfully added {catsSaved} new cats." : "No new cats were added (duplicates detected).");
        }

        
        /// <summary>
        /// Retrieves a cat by its ID.
        /// </summary>
        /// <param name="id">The ID of the cat to retrieve.</param>
        /// <returns>Returns the requested cat.</returns>
        /// <response code="200">Returns the cat.</response>
        /// <response code="404">Cat not found.</response>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCatById(int id)
        {
            // Validate that the id is a positive number
            if (id <= 0)
            {
                return BadRequest("Invalid Cat ID. ID must be greater than 0.");
            }

            var cat = await _context.Cats
                .FirstOrDefaultAsync(c=>c.Id==id);
            if(cat==null){
                return NotFound($"Cat with id:{id} not Found");
            }
            return Ok(cat); // Returns raw JSON response from TheCatAPI
        }
    

        /// <summary>
        /// Retrieves Posted Cats
        /// </summary>
        /// <param name="Tag"> Optional Filter By Tag</param>
        /// <returns> Paginated List of Cats </returns>
        /// <response code="200">Return the list of cats</response>
        /// <response code="400">Request parameters are inavalid</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<CatDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetCatsByTag([FromQuery] string? tag=null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
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

            // var cats = await query
            //     .Include(c => c.CatTags)
            //     .ThenInclude(ct => ct.TagEntity) // Include the related TagEntity for each tag
            //     .OrderBy(c => c.Id)
            //     .Skip((page - 1) * pageSize)
            //     .Take(pageSize)
            //     .ToListAsync();

            //Fix Serialization Issue
            var cats = await query
                .Include(c => c.CatTags)
                .ThenInclude(ct => ct.TagEntity)
                .OrderBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CatDto
                {
                    Id = c.Id,
                    CatId = c.CatId,
                    Width = c.Width,
                    Height = c.Height,
                    Image = c.ImageUrl,
                    Created = c.Created,
                    Tags = c.CatTags.Select(ct => ct.TagEntity.Name).ToList()
                })
                .ToListAsync();    
            //return Ok(cats);
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
