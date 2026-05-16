using System.ComponentModel.DataAnnotations;

namespace ChineseSaleApi.Models
{
    public class Package
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
        //forigen key
        [Required]
        public int LotteryId { get; set; }
        public Lottery? Lottery { get; set; }
        //ICollection
        public ICollection<PackageCart> PackageCart { get; set; } = new List<PackageCart>();
    }
}
