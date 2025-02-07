using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StealAllTheCats.Models{
    public class CatEntity
    {
        //[Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        //[Required]
        public string CatId { get; set; }

        //[Required]
        public int Width { get; set; }

        //[Required]
        public int Height { get; set; }

        //[Required]
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
     }

     
}