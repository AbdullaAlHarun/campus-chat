
using CampusChat.Models;
using Thread = CampusChat.Models.Thread;

namespace CampusChat.Services;

/// <summary>
/// Stores shared CampusChat data in memory.
/// </summary>
public sealed class CampusChatStore
{
    internal object SyncRoot { get; } = new();

    internal List<User> Users { get; } = new();
    internal List<Thread> Threads { get; } = new();
    internal List<Post> Posts { get; } = new();
    internal List<DirectChat> DirectChats { get; } = new();
    internal List<DirectMessage> DirectMessages { get; } = new();

    internal Dictionary<(int UserId, int ThreadId), int>
        LastReadPostIds { get; } = new();

    private int _nextThreadId = 1;
    private int _nextPostId = 1;
    private int _nextChatId = 1;
    private int _nextMessageId = 1;

    public CampusChatStore()
    {
        SeedUsers();
    }

    internal int NextThreadId() => _nextThreadId++;

    internal int NextPostId() => _nextPostId++;

    internal int NextChatId() => _nextChatId++;

    internal int NextMessageId() => _nextMessageId++;

    private void SeedUsers()
    {
        Users.AddRange(new[]
        {
            new User
            {
                Id = 1,
                DisplayName = "Emma",
                Bio = "First-year programming student",
                AvatarColor = "#3B82F6",
                StudyYear = 1,
                IsAuditor = false
            },
            new User
            {
                Id = 2,
                DisplayName = "Oliver",
                Bio = "Interested in C# and databases",
                AvatarColor = "#10B981",
                StudyYear = 2,
                IsAuditor = false
            },
            new User
            {
                Id = 3,
                DisplayName = "Sara",
                Bio = "Enjoys software development",
                AvatarColor = "#F59E0B",
                StudyYear = 2,
                IsAuditor = false
            },
            new User
            {
                Id = 4,
                DisplayName = "Auditor",
                Bio = "CampusChat auditor",
                AvatarColor = "#8B5CF6",
                StudyYear = 0,
                IsAuditor = true
            }
        });
    }
}
