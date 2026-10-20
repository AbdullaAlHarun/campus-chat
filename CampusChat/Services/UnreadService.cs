
using CampusChat.Models;

namespace CampusChat.Services;

public sealed class UnreadService
{
    private readonly CampusChatStore _store;
    private readonly ChangeNotifier _notifier;

    /// <summary>
    /// Creates the unread-message service.
    /// </summary>
    public UnreadService(
        CampusChatStore store,
        ChangeNotifier notifier)
    {
        _store = store;
        _notifier = notifier;
    }

    /// <summary>
    /// Returns the number of unread posts in a joined thread.
    /// </summary>
    public int GetUnreadCount(int userId, int threadId)
    {
        lock (_store.SyncRoot)
        {
            RequireUser(userId);
            var thread = RequireThread(threadId);

            if (!thread.MemberUserIds.Contains(userId))
                return 0;

            _store.LastReadPostIds.TryGetValue(
                (userId, threadId),
                out int lastReadPostId);

            return _store.Posts.Count(p =>
                p.ThreadId == threadId &&
                p.Id > lastReadPostId &&
                p.AuthorUserId != userId);
        }
    }

    /// <summary>
    /// Marks all current posts in a joined thread as read.
    /// </summary>
    public void MarkAsRead(int userId, int threadId)
    {
        lock (_store.SyncRoot)
        {
            RequireUser(userId);
            var thread = RequireThread(threadId);

            if (!thread.MemberUserIds.Contains(userId))
                return;

            int latestPostId = _store.Posts
                .Where(p => p.ThreadId == threadId)
                .Select(p => p.Id)
                .DefaultIfEmpty(0)
                .Max();

            _store.LastReadPostIds[(userId, threadId)] = latestPostId;
        }

        _notifier.NotifyChanged();
    }

    /// <summary>
    /// Returns unread counts for all threads joined by a user.
    /// </summary>
    public Dictionary<int, int> GetUnreadCounts(int userId)
    {
        lock (_store.SyncRoot)
        {
            RequireUser(userId);

            var joinedThreadIds = _store.Threads
                .Where(t => t.MemberUserIds.Contains(userId))
                .Select(t => t.Id)
                .ToList();

            var result = new Dictionary<int, int>();

            foreach (int threadId in joinedThreadIds)
            {
                result[threadId] = GetUnreadCount(userId, threadId);
            }

            return result;
        }
    }

    private User RequireUser(int userId)
    {
        return _store.Users.FirstOrDefault(u => u.Id == userId)
            ?? throw new KeyNotFoundException("User not found.");
    }

    private CampusChat.Models.Thread RequireThread(int threadId)
    {
        return _store.Threads.FirstOrDefault(t => t.Id == threadId)
            ?? throw new KeyNotFoundException("Thread not found.");
    }
}
