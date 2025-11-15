using System.ComponentModel.DataAnnotations;

namespace PROG6212_POE.Models
{
    public class User
    {
        public int UserId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Surname { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Range(100, 1000, ErrorMessage = "Hourly rate must be between R100 and R1000.")]
        public decimal HourlyRate { get; set; }

        [Required]
        public string Role { get; set; } = string.Empty; // "HR", "Lecturer", "Coordinator", "Manager"

        // Authentication fields (simplified for prototype)
        public string Password { get; set; } = string.Empty; // In production, use hashed passwords

        // Navigation properties
        public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();
    }
}
