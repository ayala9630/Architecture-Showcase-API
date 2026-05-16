using System;
using System.ComponentModel.DataAnnotations;

namespace ChineseSaleApi.Dto
{
    public class DonorDto
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(30)]
        public string? FirstName { get; set; }
        [MaxLength(30)]
        public string? LastName { get; set; }
        [MaxLength(30)]
        [Required]
        public string CompanyName { get; set; }
        [Required]
        [EmailAddress]
        public string CompanyEmail { get; set; }
        [Phone]
        public string? CompanyPhone { get; set; }
        [MaxLength(250)]
        public string? CompanyIcon { get; set; }
        public int CompanyAddressId { get; set; }
    }
    public class SingelDonorDto
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(30)]
        public string? FirstName { get; set; }
        [MaxLength(30)]
        public string? LastName { get; set; }
        [MaxLength(30)]
        [Required]
        public string CompanyName { get; set; } 
        public string? CompanyIcon { get; set; }
        public IDictionary<string,int>? Gifts { get; set; }
    }
    public class CreateDonorDto
    {
        [MaxLength(30)]
        public string? FirstName { get; set; }
        [MaxLength(30)]
        public string? LastName { get; set; }
        [Required]
        [MaxLength(30)]
        public string CompanyName { get; set; }
        [Required]
        [EmailAddress]
        public string CompanyEmail { get; set; }
        [Phone]
        public string? CompanyPhone { get; set; }
        [MaxLength(250)]
        public string? CompanyIcon { get; set; }
        public CreateAddressForDonorDto CompanyAddress { get; set; } = null!;
    }
    public class UpdateDonorDto
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(30)]
        public string? FirstName { get; set; }
        [MaxLength(30)]
        public string? LastName { get; set; }
        [MaxLength(30)]
        public string? CompanyName { get; set; }
        [EmailAddress]
        public string? CompanyEmail { get; set; }
        [Phone]
        public string? CompanyPhone { get; set; }
        [MaxLength(250)]
        public string? CompanyIcon { get; set; }
        public int? CompanyAddressId { get; set; }
    }
}