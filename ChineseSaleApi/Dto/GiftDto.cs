using System;
using System.ComponentModel.DataAnnotations;

namespace ChineseSaleApi.Dto
{
    public class CreateGiftDto
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        [MaxLength(250)]
        public string? Description { get; set; }
        public int? Price { get; set; }
        public int? GiftValue { get; set; }
        [MaxLength(250)]
        public string? ImageUrl { get; set; }
        public bool? IsPackageAble { get; set; }
        public int DonorId { get; set; }
        public int? CategoryId { get; set; }
        public int LotteryId { get; set; }
    }
    public class UpdateGiftDto
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(50)]
        public string? Name { get; set; }
        [MaxLength(250)]
        public string? Description { get; set; }
        public int? Price { get; set; }
        public int? GiftValue { get; set; }
        [MaxLength(250)]
        public string? ImageUrl { get; set; }
        public bool? IsPackageAble { get; set; }
        public int? DonorId { get; set; }
        public int? CategoryId { get; set; }
        public int LotteryId { get; set; }
    }
    public class GiftDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        public string? Description { get; set; }
        public int? Price { get; set; }
        public int? GiftValue { get; set; }
        [MaxLength(250)]
        public string? ImageUrl { get; set; }
        public bool? IsPackageAble { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyLogoUrl { get; set; }
        public string? CategoryName { get; set; }
        public int LotteryId { get; set; }
        public string? winner { get; set; }

    }
    public class GiftWithOldPurchaseDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        public int? Price { get; set; }
        public int? GiftValue { get; set; }
        [MaxLength(250)]
        public string? ImageUrl { get; set; }
        public bool? IsPackageAble { get; set; }
        public int OldPurchaseCount { get; set; }
        public string? CategoryName { get; set; }
        public string? winner { get; set; }
        public int? countCards { get; set; }
    }

}