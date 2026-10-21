using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserRole.Models
{
    public class Users : IdentityUser
    {
        // =========================
        // PERSONAL INFORMATION
        // =========================

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string EnrollmentNo { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        [NotMapped]
        public string? Role { get; set; }

        [StringLength(500)]
        public string? Bio { get; set; }

        public string? ProfilePicture { get; set; }

        // =========================
        // OPTIONAL PROFILE DETAILS
        // =========================

        [StringLength(10)]
        public string? Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(150)]
        public string? Skills { get; set; }

        [StringLength(200)]
        public string? GitHubUrl { get; set; }

        [StringLength(200)]
        public string? LinkedInUrl { get; set; }

        [StringLength(200)]
        public string? PortfolioUrl { get; set; }

        // =========================
        // ACCOUNT INFORMATION
        // =========================

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;
        // =========================
        // ONLINE STATUS
        // =========================

        public bool IsOnline { get; set; } = false;

        public DateTime? LastSeen { get; set; }

        [StringLength(100)]
        public string? ConnectionId { get; set; }

        // =========================
        // NAVIGATION PROPERTIES
        // =========================

        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();

        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        // =========================
        // DISPLAY HELPERS
        // =========================

        public string DisplayName =>
            string.IsNullOrWhiteSpace(FullName) ? UserName ?? "User" : FullName;

        public string ProfileImage =>
            string.IsNullOrWhiteSpace(ProfilePicture)
                ? "/images/default-user.png"
                : ProfilePicture;
    }
}
