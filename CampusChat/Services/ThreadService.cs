
using CampusChat.Models;
using Thread = CampusChat.Models.Thread;

namespace CampusChat.Services;

public sealed class ThreadService
{
    private readonly CampusChatStore _store;
    private readonly ChangeNotifier _notifier;

    /// <summary>
    /// Creates a service for managing discussion threads.
    /// </summary>
    public ThreadService(CampusChatStore store, ChangeNotifier notifier)
    {
        _store = store;
        _notifier = notifier;
    }

    /// <summary>
    /// Returns all threads without exposing access codes.
    /// </summary>
    public List<Thread> GetThreads()
    {
        lock (_store.SyncRoot)
        {
            return _store.Threads.Select(CopyThread).ToList();
        }
    }

    /// <summary>
    /// Returns a thread by ID without exposing its access code.
    /// </summary>
    public Thread? GetThreadById(int threadId)
    {
        lock (_store.SyncRoot)
        {
            var thread = FindThread(threadId);
            return thread == null ? null : CopyThread(thread);
        }
    }

    /// <summary>
    /// Searches threads by title or description.
    /// </summary>
    public List<Thread> SearchThreads(string? searchText)
    {
        lock (_store.SyncRoot)
        {
            var query = searchText?.Trim() ?? string.Empty;

            return _store.Threads
                .Where(t =>
                    t.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    t.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Select(CopyThread)
                .ToList();
        }
    }

    /// <summary>
    /// Creates a public or private thread.
    /// </summary>
    public Thread CreateThread(
        int userId,
        string title,
        string description,
        bool isPrivate,
        string? accessCode)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Thread title cannot be empty.");

        if (isPrivate && string.IsNullOrWhiteSpace(accessCode))
            throw new ArgumentException("Private threads require an access code.");

        Thread result;

        lock (_store.SyncRoot)
        {
            RequireUser(userId);

            var thread = new Thread
            {
                Id = _store.NextThreadId(),
                Title = title.Trim(),
                Description = description?.Trim() ?? string.Empty,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow,
                IsPrivate = isPrivate,
                AccessCode = isPrivate ? accessCode!.Trim() : null,
                MemberUserIds = new List<int> { userId }
            };

            _store.Threads.Add(thread);
            result = CopyThread(thread);
        }

        _notifier.NotifyChanged();
        return result;
    }

    /// <summary>
    /// Joins a thread after validating its access requirements.
    /// </summary>
    public void JoinThread(int userId, int threadId, string? accessCode = null)
    {
        lock (_store.SyncRoot)
        {
            var user = RequireUser(userId);
            var thread = RequireThread(threadId);

            if (thread.MemberUserIds.Contains(userId))
                return;

            if (thread.IsPrivate &&
                !user.IsAuditor &&
                thread.AccessCode != accessCode?.Trim())
            {
                throw new UnauthorizedAccessException(
                    "Incorrect private thread access code.");
            }

            thread.MemberUserIds.Add(userId);

            var latestPostId = _store.Posts
                .Where(p => p.ThreadId == threadId)
                .Select(p => p.Id)
                .DefaultIfEmpty(0)
                .Max();

            _store.LastReadPostIds[(userId, threadId)] = latestPostId;
        }

        _notifier.NotifyChanged();
    }

    /// <summary>
    /// Removes a user from a joined thread.
    /// </summary>
    public void LeaveThread(int userId, int threadId)
    {
        bool changed;

        lock (_store.SyncRoot)
        {
            RequireUser(userId);
            var thread = RequireThread(threadId);

            changed = thread.MemberUserIds.Remove(userId);
            _store.LastReadPostIds.Remove((userId, threadId));
        }

        if (changed)
            _notifier.NotifyChanged();
    }

    /// <summary>
    /// Returns threads joined by a particular user.
    /// </summary>
    public List<Thread> GetJoinedThreads(int userId)
    {
        lock (_store.SyncRoot)
        {
            RequireUser(userId);

            return _store.Threads
                .Where(t => t.MemberUserIds.Contains(userId))
                .Select(CopyThread)
                .ToList();
        }
    }

    /// <summary>
    /// Checks whether a user may read a thread's posts.
    /// </summary>
    public bool CanReadThread(int userId, int threadId)
    {
        lock (_store.SyncRoot)
        {
            var user = RequireUser(userId);
            var thread = RequireThread(threadId);

            return user.IsAuditor ||
                   !thread.IsPrivate ||
                   thread.MemberUserIds.Contains(userId);
        }
    }

    private User RequireUser(int userId)
    {
        return _store.Users.FirstOrDefault(u => u.Id == userId)
            ?? throw new KeyNotFoundException("User not found.");
    }

    private Thread RequireThread(int threadId)
    {
        return FindThread(threadId)
            ?? throw new KeyNotFoundException("Thread not found.");
    }

    private Thread? FindThread(int threadId)
    {
        return _store.Threads.FirstOrDefault(t => t.Id == threadId);
    }

    private static Thread CopyThread(Thread thread)
    {
        return new Thread
        {
            Id = thread.Id,
            Title = thread.Title,
            Description = thread.Description,
            CreatedByUserId = thread.CreatedByUserId,
            CreatedAt = thread.CreatedAt,
            IsPrivate = thread.IsPrivate,
            AccessCode = null,
            MemberUserIds = new List<int>(thread.MemberUserIds)
        };
    }
}
