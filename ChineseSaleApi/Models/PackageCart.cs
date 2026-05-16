using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChineseSaleApi.Models
{
    public class PackageCart
    {
        [Key]
        public int Id { get; set; }
        public int Quantity { get; set; } = 1;
        //forigen key
        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User? User { get; set; }
        [Required]
        [ForeignKey("Package")]
        public int PackageId { get; set; }
        public Package? Package { get; set; }
    }
}
