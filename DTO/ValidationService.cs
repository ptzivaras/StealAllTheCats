using System;
using System.ComponentModel.DataAnnotations;

namespace StealAllTheCats.DTOs
{
    public class CatDTO
    {
        [Required(ErrorMessage = "CatId is required.")]
        [StringLength(100, ErrorMessage = "CatId can't be longer than 100 characters.")]
        public string CatId { get; set; }

        [Range(1, 5000, ErrorMessage = "Width must be between 1 and 5000.")]
        public int Width { get; set; }

        [Range(1, 5000, ErrorMessage = "Height must be between 1 and 5000.")]
        public int Height { get; set; }

        [Required(ErrorMessage = "Image URL is required.")]
        [Url(ErrorMessage = "Image URL must be a valid URL.")]
        public string ImageUrl { get; set; }

        //[Required(ErrorMessage = "Temperament is required.")]
        //public string Temperament { get; set; }

        //public DateTime Created { get; set; } = DateTime.UtcNow;
    }


    // // You can also create custom validation methods, for example:
    // public class TemperamentValidator : ValidationAttribute
    // {
    //     //Affectionate, Intelligent, Playful,Alert, Agile, Energetic, Demanding
    //     //Dependent, Gentle, Lively, Loyal, Sweet, Quiet, Peaceful
    //     //Friendly, Social, Loving
    //     private readonly string[] _validTemperaments = { "Playful", "Aggressive", "Friendly", "Curious", "Calm"};

    //     public override bool IsValid(object value)
    //     {
    //         if (value is string temperament)
    //         {
    //             return Array.Exists(_validTemperaments, t => t.Equals(temperament, StringComparison.OrdinalIgnoreCase));
    //         }
    //         return false;
    //     }
    // }
}
