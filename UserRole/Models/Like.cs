using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserRole.Models
{
    public class Like
    {
        [Key]
        public int Id { get; set; }

        // =========================
        // POST RELATION
        // =========================

        [Required]
        public int PostId { get; set; }

        [ForeignKey(nameof(PostId))]
        public virtual Post? Post { get; set; }

        // =========================
        // USER RELATION
        // =========================

        [Required]
        public string UserId { get; set; } = string.Empty;

        
        public virtual Users? User { get; set; }

        // =========================
        // CREATED DATE
        // =========================

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}