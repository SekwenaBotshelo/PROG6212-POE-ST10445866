using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROG6212_POE.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "First Name")]
        [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Surname is required")]
        [Display(Name = "Last Name")]
        [StringLength(50, ErrorMessage = "Surname cannot exceed 50 characters")]
        public string Surname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hourly rate is required")]
        [Range(100, 1000, ErrorMessage = "Hourly rate must be between R100 and R1000")]
        [Display(Name = "Hourly Rate (R)")]
        public decimal HourlyRate { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; } = string.Empty; // "HR", "Lecturer", "Coordinator", "Manager"

        // Authentication fields (simplified for prototype)
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<Claim> ClaimsSubmitted { get; set; } = new List<Claim>();
        public virtual ICollection<Claim> ClaimsVerified { get; set; } = new List<Claim>();
        public virtual ICollection<Claim> ClaimsApproved { get; set; } = new List<Claim>();

        // Helper properties
        [NotMapped]
        public string FullName => $"{Name} {Surname}";

        [NotMapped]
        public string DisplayRole => Role switch
        {
            "HR" => "HR Manager",
            "Lecturer" => "Lecturer",
            "Coordinator" => "Programme Coordinator",
            "Manager" => "Academic Manager",
            _ => Role
        };
    }
}