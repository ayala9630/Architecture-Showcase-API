using System;
using System.ComponentModel.DataAnnotations;

namespace ChineseSaleApi.Dto
{
    public class AddressDto
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
    }
    public class CreateAddressForUserDto
    {
        [Required]
        [MaxLength(50)]
        public string City { get; set; }
        [Required]
        [MaxLength(50)]
        public string Street { get; set; } 
        public int? Number { get; set; }
        public int? ZipCode { get; set; }
        public int UserId { get; set; }
    }
    public class CreateAddressForDonorDto
    {
        [Required]
        [MaxLength(50)]
        public string City { get; set; }
        [Required]
        [MaxLength(50)]
        public string Street { get; set; }
        public int? Number { get; set; }
        public int? ZipCode { get; set; }
        //public int? DonorId { get; set; }
    }
}