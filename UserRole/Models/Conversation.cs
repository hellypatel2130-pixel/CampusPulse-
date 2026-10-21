using System.ComponentModel.DataAnnotations.Schema;

namespace UserRole.Models
{
    public class Conversation
    {
        public int Id { get; set; }

        // =========================
        // FIRST USER
        // =========================

        public string User1Id { get; set; } = string.Empty;

        [ForeignKey(nameof(User1Id))]
        public virtual Users? User1 { get; set; }

        // =========================
        // SECOND USER
        // =========================

        public string User2Id { get; set; } = string.Empty;

        [ForeignKey(nameof(User2Id))]
        public virtual Users? User2 { get; set; }

        // =========================
        // CREATED DATE
        // =========================

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // =========================
        // MESSAGES
        // =========================

        public virtual ICollection<Message> Messages { get; set; }
            = new List<Message>();
    }
}