using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Text.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Data; 
using StealAllTheCats.Models;
using StealAllTheCats.Data;
//using StealAllTheCats.DTOs;
using System.ComponentModel.DataAnnotations;
using StealAllTheCats.Dtos;

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
 
        //First Endpoint(Post 25 cats in LocalServer after you get them from Cat Server)
        //POST /api/cats/fetch
        
        [HttpPost("fetch")]
        public async Task<IActionResult> SaveUniqueCats()
        {
            string apiKey = "live_r28aR40CbiGE2ucr7fiQVsNiCfACtX0VopUMAMWk1YCxiUxOCgIB06gUcsr3vwrN"; // Replace with your actual API key
            string apiUrl = $"https://api.thecatapi.com/v1/images/search?limit=25&api_key={apiKey}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Failed to fetch cats from TheCatAPI");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var fetchedCats = JsonSerializer.Deserialize<List<CatApiResponse>>(jsonResponse);

            if (fetchedCats == null || fetchedCats.Count == 0)
            {
                return BadRequest("No cats found.");
            }

            // Convert API response into `CatEntity` and save tags
            var newCats = new List<CatEntity>();

            foreach (var cat in fetchedCats)
            {
                // Skip cats with no temperament
                if (cat.breeds == null || cat.breeds.Count == 0 || string.IsNullOrEmpty(cat.breeds[0].temperament))
                    continue;

                // Save the cat
                var newCat = new CatEntity
                {
                    CatId = cat.id,
                    Width = cat.width,
                    Height = cat.height,
                    ImageUrl = cat.url,
                    Created = DateTime.UtcNow
                };

                // Manually validate the newCat object
                var validationResults = new List<ValidationResult>();
                var isValid = Validator.TryValidateObject(newCat, new ValidationContext(newCat), validationResults, true);

                if (!isValid)
                {
                    // Collect all validation error messages
                    var errorMessages = validationResults.Select(vr => vr.ErrorMessage).ToList();
                    return BadRequest(new { Errors = errorMessages });
                }

                // Get the temperament tags
                var tags = cat.breeds[0].temperament.Split(',')
                    .Select(t => t.Trim()) // Trim any extra spaces
                    .Where(t => !string.IsNullOrEmpty(t)) // Ensure non-empty tags
                    .ToList();
                
                var newTags = new List<TagEntity>();

                foreach (var tagName in tags)
                {
                    // Validate tag name before saving
                    if (string.IsNullOrEmpty(tagName))
                    {
                        continue; // Skip invalid tags
                    }
                    // Check if the tag already exists
                    var existingTag = await _context.Tags
                        .FirstOrDefaultAsync(t => t.Name == tagName);

                    if (existingTag == null)
                    {
                        // If not, add the new tag
                        var newTag = new TagEntity
                        {
                            Name = tagName,
                            Created = DateTime.UtcNow
                        };

                        // Validate the new tag object before adding it to DB
                        var tagValidationResults = new List<ValidationResult>();
                        var isTagValid = Validator.TryValidateObject(newTag, new ValidationContext(newTag), tagValidationResults, true);

                        if (!isTagValid)
                        {
                            var tagErrorMessages = tagValidationResults.Select(vr => vr.ErrorMessage).ToList();
                            return BadRequest(new { Errors = tagErrorMessages });
                        }

                        _context.Tags.Add(newTag);
                        newTags.Add(newTag); // Add the new tag to the list of tags
                    }
                    else
                    {
                        newTags.Add(existingTag); // Add the existing tag to the list
                    }
                }

                // Add the cat to the list
                newCats.Add(newCat);

                // Create relationships (CatTag) in the pivot table
                foreach (var tag in newTags)
                {
                    _context.CatTags.Add(new CatTag
                    {
                        CatEntity = newCat,
                        TagEntity = tag
                    });
                }
            }

            // Check for duplicates before inserting
            var existingCatIds = _context.Cats.Select(c => c.CatId).ToList();
            var uniqueCats = newCats.Where(c => !existingCatIds.Contains(c.CatId)).ToList();

            if (uniqueCats.Any())
            {
                _context.Cats.AddRange(uniqueCats);
                await _context.SaveChangesAsync();
                return Ok($"Successfully added {uniqueCats.Count} new cats.");
            }

            return Ok("No new cats were added (duplicates detected or no temperament).");
        }



        //Second EndPoint(Get Cat By id)
        //GET /api/cats/{id}:
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
    


        //Third and Fourth Endpoint(Retrieve cats with a specific tag and paging support)
        //GET /api/cats?tag=playful&page=1&pageSize=10
        [HttpGet]
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
