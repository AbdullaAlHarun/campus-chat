namespace CampusChat.Models;

public class Post
{
    public int Id { get; set; }

    public int ThreadId { get; set; }

    public int AuthorUserId { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? ParentPostId { get; set; }
}