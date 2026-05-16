using ChineseSaleApi.Validations;
using System;
using System.ComponentModel.DataAnnotations;

namespace ChineseSaleApi.Dto
{
    public class LotteryDto
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = null!;
        [Required]
        [DateValidation]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        public int? TotalCards { get; set; }
        public int? TotalSum { get; set; }
        public bool? IsDone { get; set; }
    }
    public class CreateLotteryDto
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        [Required]
        [DateValidation]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
    }
    public class UpdateLotteryDto
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; } = null!;
        [DateValidation]
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? TotalCards { get; set; }
        public int? TotalSum { get; set; }
        public bool? IsDone { get; set; }
    }
    
}