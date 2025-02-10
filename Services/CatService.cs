using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using StealAllTheCats.Data;
using StealAllTheCats.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StealAllTheCats.Services
{
    public class CatService
    {
        private readonly ApplicationDbContext _context;

        public CatService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveCatsToDatabaseAsync(List<CatEntity> newCats)
        {
            Console.WriteLine($"[DEBUG] Service received {newCats.Count} cats for saving");

            if (!newCats.Any()) 
            {
                Console.WriteLine("[DEBUG] No new cats in request.");
                return 0; // No new cats to add.
            }

            //Get existing cat IDs from the database
            var existingCatIds = await _context.Cats.Select(c => c.CatId).ToListAsync();
            Console.WriteLine($"[DEBUG] Existing Cat IDs in DB: {string.Join(", ", existingCatIds)}");

            //Find unique cats
            var uniqueCats = newCats.Where(c => !existingCatIds.Contains(c.CatId)).ToList();
            Console.WriteLine($"[DEBUG] Unique Cats to be added: {string.Join(", ", uniqueCats.Select(c => c.CatId))}");

            if (!uniqueCats.Any())
            {
                Console.WriteLine("[DEBUG] No unique cats to add (all were duplicates).");
                return 0;
            }

            //Get all existing tags from the database
            var allTagNames = uniqueCats
                .SelectMany(c => c.CatTags.Select(ct => ct.TagEntity.Name))
                .Distinct()
                .ToList();

            var existingTags = await _context.Tags
                .Where(t => allTagNames.Contains(t.Name))
                .ToListAsync();

            //Store new tags temporarily before adding
            var newTags = new List<TagEntity>();

            foreach (var newCat in uniqueCats)
            {
                foreach (var catTag in newCat.CatTags)
                {
                    var tag = catTag.TagEntity;

                    //Check if the tag already exists
                    var existingTag = existingTags.FirstOrDefault(t => t.Name == tag.Name);
                    if (existingTag != null)
                    {
                        catTag.TagEntity = existingTag; //Reuse the existing tag
                    }
                    else
                    {
                        newTags.Add(tag); //Store new tags to add later
                    }
                }
            }

            if (newTags.Any())
            {
                _context.Tags.AddRange(newTags);
                await _context.SaveChangesAsync();
            }

            _context.Cats.AddRange(uniqueCats);
            await _context.SaveChangesAsync();

            Console.WriteLine($"[DEBUG] Successfully saved {uniqueCats.Count} new cats.");
            return uniqueCats.Count;
        }

 
  }
}
