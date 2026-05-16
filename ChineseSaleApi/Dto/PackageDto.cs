using System;
using System.ComponentModel.DataAnnotations;

namespace ChineseSaleApi.Dto
{
    public class PackageDto
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        [MaxLength(250)]
        public string? Description { get; set; }
        [MaxLength(500)]
        public string? ImageUrl { get; set; }
        [Required]
        public int NumOfCards { get; set; }
        [Required]
        public int Price { get; set; }
        [Required]
        public int LotteryId { get; set; }
    }
    public class CreatePackageDto
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        [MaxLength(250)]
        public string? Description { get; set; }
        [MaxLength(500)]
        public string? ImageUrl { get; set; }
        [Required]
        public int NumOfCards { get; set; }
        [Required]
        public int Price { get; set; }
        [Required]
        public int LotteryId { get; set; }
    }
    public class UpdatePackageDto
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(50)]
        public string? Name { get; set; }
        [MaxLength(250)]
        public string? Description { get; set; }
        [MaxLength(500)]
        public string? ImageUrl { get; set; }
        public int? NumOfCards { get; set; }
        public int? Price { get; set; }
        public int? LotteryId { get; set; }
    }
}