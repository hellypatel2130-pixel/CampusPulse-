using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserRole.Models
{
    public class Message
    {
        public int Id { get; set; }

        // ==========================================
        // Conversation
        // ==========================================

        public int ConversationId { get; set; }

        [ForeignKey(nameof(ConversationId))]
        public virtual Conversation? Conversation { get; set; }

        // ==========================================
        // Sender
        // ==========================================

        public string SenderId { get; set; } = string.Empty;

        [ForeignKey(nameof(SenderId))]
        public virtual Users? Sender { get; set; }

        // ==========================================
        // Text Message
        // ==========================================

        public string Text { get; set; } = string.Empty;

        // ==========================================
        // File Attachment
        // ==========================================

        public string? FileName { get; set; }

        public string? FilePath { get; set; }

        public string? FileType { get; set; }

        public long? FileSize { get; set; }

        // ==========================================
        // Shared Post
        // ==========================================

        public int? SharedPostId { get; set; }

        [ForeignKey(nameof(SharedPostId))]
        public virtual Post? SharedPost { get; set; }

        // ==========================================
        // Date & Status
        // ==========================================

        public DateTime SentAt { get; set; } = DateTime.Now;

        public bool IsSeen { get; set; } = false;
    }
}