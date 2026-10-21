using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserRole.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        // =========================
        // RECEIVER USER
        // =========================

        [Required]
        public string UserId { get; set; } = string.Empty;

        
        public virtual Users? User { get; set; }

        // =========================
        // NOTIFICATION DETAILS
        // =========================

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        // URL to open when notification is clicked
        [StringLength(500)]
        public string Url { get; set; } = string.Empty;

        // Example:
        // Like
        // Comment
        // Follow
        // Message
        // Event
        // Announcement
        [StringLength(50)]
        public string NotificationType { get; set; } = "General";

        // Optional icon (FontAwesome class)
        [StringLength(100)]
        public string Icon { get; set; } = "fa-solid fa-bell";

        // Optional Bootstrap color
        [StringLength(30)]
        public string Color { get; set; } = "primary";

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}