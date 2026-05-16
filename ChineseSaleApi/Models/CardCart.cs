using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChineseSaleApi.Models
{
    public class CardCart
    {
        [Key]
        public int Id { get; set; }
        public int Quantity { get; set; }=1;
        //forigen key
        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User? User { get; set; }
        [Required]
        [ForeignKey("Gift")]
        public int GiftId { get; set; }
        public Gift? Gift { get; set; }
    }
}
