using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UserRole.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required]
        public string Caption { get; set; } = string.Empty;

        // ==========================================
        // Uploaded File
        // ==========================================

        public string FilePath { get; set; } = string.Empty;

        public string FileType { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;

        // ==========================================
        // Status
        // ==========================================

        public bool IsHidden { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // ==========================================
        // Owner
        // ==========================================

        [Required]
        public string UserId { get; set; } = string.Empty;

        public virtual Users? Users { get; set; }

        // ==========================================
        // Likes & Comments
        // ==========================================

        public virtual List<Like> Likes { get; set; } = new();

        public virtual List<Comment> Comments { get; set; } = new();

        // ==========================================
        // Shared in Chat (Instagram Style)
        // ==========================================

      
    }
}