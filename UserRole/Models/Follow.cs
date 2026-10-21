using System.ComponentModel.DataAnnotations.Schema;

namespace UserRole.Models
{
    public class Follow
    {
        public int Id { get; set; }

        public string FollowerId { get; set; } = string.Empty;

        [ForeignKey("FollowerId")]
        public Users? Follower { get; set; }

        public string FollowingId { get; set; } = string.Empty;

        [ForeignKey("FollowingId")]
        public Users? Following { get; set; }

        public bool IsAccepted { get; set; }

        public DateTime CreatedAt { get; set; }
            = DateTime.Now;
    }
}