using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StealAllTheCats.Models{
    public class CatEntity
    {
        //[Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "CatId is required.")]
        public string CatId { get; set; }

        [Range(1, 5000, ErrorMessage = "Width must be between 1 and 5000.")]
        public int Width { get; set; }

        [Range(1, 5000, ErrorMessage = "Height must be between 1 and 5000.")]
        public int Height { get; set; }

        [Required(ErrorMessage = "Image URL is required.")]
        [Url(ErrorMessage = "Invalid image URL.")]
        public string ImageUrl { get; set; }

        public DateTime Created { get; set; } = DateTime.UtcNow;

        public List<CatTag> CatTags { get; set; } = new List<CatTag>();  // Join table
    }

     //DTO used for Post to take the response
     public class CatApiResponse
     {
         public string id { get; set; }
         public string url { get; set; }
         public int width { get; set; }
         public int height { get; set; }
         public string temperament {get;set;}
         public List<Breed> breeds { get; set; }
     }

     public class Breed
{
    public string name { get; set; }
    public string temperament { get; set; }  // The temperament for each breed
}

     
}