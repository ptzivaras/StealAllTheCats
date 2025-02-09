using System;
using System.ComponentModel.DataAnnotations;

namespace StealAllTheCats.Dtos
{
    public class TagDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
    }
}