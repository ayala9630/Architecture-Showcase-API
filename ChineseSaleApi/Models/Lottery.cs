using ChineseSaleApi.Validations;
using System.ComponentModel.DataAnnotations;

namespace ChineseSaleApi.Models
{
    public class Lottery
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        [Required]
        [DateValidation]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        public int? TotalCards { get; set; }
        public int? TotalSum { get; set; }
        public bool IsDone { get; set; } = false;
        //ICollection
        public ICollection<Package> Packages { get; set; } = new List<Package>();
        public ICollection<Gift> Gifts { get; set; }= new List<Gift>();
        public ICollection<Donor> Donors { get; set; }= new List<Donor>();
    }
}
