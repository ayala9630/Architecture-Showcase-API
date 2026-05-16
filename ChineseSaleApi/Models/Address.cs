using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChineseSaleApi.Models
{
    public class Address
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string City { get; set; }
        [Required]
        [MaxLength(50)]
        public string Street { get; set; }
        public int? Number { get; set; }
        public int? ZipCode { get; set; }
        //forigen key
        [ForeignKey("User")]
        public int? UserId { get; set; }
        public User? User { get; set; }
        [ForeignKey("Donor")]
        public int? DonorId { get; set; }
        public Donor? Donor { get; set; }

    }
}
