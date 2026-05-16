using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChineseSaleApi.Models
{
    public class Card
    {
        [Key]
        public int Id { get; set; }
        public bool? IsWin { get; set; } = false;
        //forigen key
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User? User { get; set; }
        [Required]
        [ForeignKey("Gift")]
        public int GiftId { get; set; }
        public Gift? Gift { get; set; }
    }
}
