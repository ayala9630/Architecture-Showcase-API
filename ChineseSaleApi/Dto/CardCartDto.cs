using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChineseSaleApi.Dto
{
    public class CardCartDto
    {
        [Key]
        public int Id { get; set; }
        public int Quantity { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public int GiftId { get; set; }
    }
    public class CreateCardCartDto
    {
        [Key]
        public int Id { get; set; }
        public int Quantity { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public int GiftId { get; set; }
    }
}