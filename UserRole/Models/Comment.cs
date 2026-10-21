using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserRole.Models
{
    public class Comment
    {
        public int Id { get; set; }

        //==================================================
        // COMMENT
        //==================================================

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        //==================================================
        // DATES
        //==================================================

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        //==================================================
        // STATUS
        //==================================================

        public bool IsEdited { get; set; } = false;

        public bool IsDeleted { get; set; } = false;

        //==================================================
        // POST
        //==================================================

        [Required]
        public int PostId { get; set; }

        [ForeignKey(nameof(PostId))]
        public virtual Post? Post { get; set; }

        //==================================================
        // USER
        //==================================================

        [Required]
        public string UserId { get; set; } = string.Empty;

        // Fix: Changed property name from 'User' to 'Users' to match class type and avoid shadow columns
       
        public virtual Users? Users { get; set; }
    }
}