using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChineseSaleApi.Models
{
    public class Gift
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        [MaxLength(250)]
        public string? Description { get; set; }
        public int? Price { get; set; }
        public int? GiftValue { get; set; }
        [MaxLength(250)]
        public string? ImageUrl { get; set; }
        public bool? IsPackageAble { get; set; } = true;
        //forigen key
        [ForeignKey("Donor")]
        public int DonorId { get; set; }
        public Donor? Donor { get; set; }
        [ForeignKey("Category")]
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }
        [ForeignKey("Lottery")]
        public int LotteryId { get; set; }
        public Lottery? Lottery { get; set; }
        //Icollections
        public ICollection<CardCart> CartCarts { get; set; }=new List<CardCart>();
        public ICollection<Card> Cards { get; set; } = new List<Card>();
        //public ICollection<User> Purchasers { get; set; } = new List<User>();
    }
}
