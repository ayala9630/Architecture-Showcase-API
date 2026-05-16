using System;
using System.ComponentModel.DataAnnotations;

namespace ChineseSaleApi.Dto
{
    public class CategoryDto
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
    }
    public class CreateCategoryDto
    {
        [Required]
        public string Name { get; set; }
    }
}