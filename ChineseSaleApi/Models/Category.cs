using System.ComponentModel.DataAnnotations;

namespace ChineseSaleApi.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(30)]
        public string Name { get; set; }
        //ICollection
        public ICollection<Gift> Gifts { get; set; }= new List<Gift>();
    }
}
