namespace CampusChat.Models;

public class User
{
    public int Id { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string Bio { get; set; } = string.Empty;

    public string AvatarColor { get; set; } = "#3B82F6";

    public int StudyYear { get; set; }

    public bool IsAuditor { get; set; }
}