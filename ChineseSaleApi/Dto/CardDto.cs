using System;
using System.ComponentModel.DataAnnotations;

namespace ChineseSaleApi.Dto
{
    public class ListCardDto
    {
        public string GiftName { get; set; }
        public string ImageUrl { get; set; }
        public int Quantity { get; set; }
        [Required]
        public int GiftId { get; set; }
        public int? GiftValue { get; set; }
        public UserDto WinUser { get; set; }
    }
    public class CardDto
    {
        [Required]
        public int GiftId { get; set; }
        public string? GiftName { get; set; }
        public IDictionary<string, int>? CardPurchases { get; set; }
        public string? WinnerName { get; set; }
    }
    public class CreateCardDto
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public int GiftId { get; set; }
    }
    public class UpdateCardDto
    {
        [Key]
        public int Id { get; set; }
        public bool? IsWin { get; set; } = true;
    }

}