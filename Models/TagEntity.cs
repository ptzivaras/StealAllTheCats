using System.ComponentModel.DataAnnotations;


public class TagEntity
{
    public int Id { get; set; }
    
    public string Name { get; set; }
    public DateTime Created { get; set; } = DateTime.UtcNow;

    public List<CatTag> CatTags { get; set; } = new List<CatTag>();  // Join table
}