namespace CampusChat.Models;

public class Thread
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int CreatedByUserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsPrivate { get; set; }

    public string? AccessCode { get; set; }

    public List<int> MemberUserIds { get; set; } = new();
}