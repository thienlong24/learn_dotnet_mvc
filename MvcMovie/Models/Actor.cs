using System.ComponentModel.DataAnnotations;

namespace MvcMovie.Models
{
    public class Actor
    {
        public int Id { get; set; }

        [Display(Name = "Full Name")]
        [StringLength(60, MinimumLength = 1)]
        [Required]
        public string? FullName { get; set; }

        [Display(Name = "Date Of Birth")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public Gender Gender { get; set; }
    }
}