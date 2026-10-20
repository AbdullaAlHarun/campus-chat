
using CampusChat.Models;

namespace CampusChat.Services;

public sealed class PostService
{
    private readonly CampusChatStore _store;
    private readonly ChangeNotifier _notifier;

    /// <summary>
    /// Creates the post service.
    /// </summary>
    public PostService(CampusChatStore store, ChangeNotifier notifier)
    {
        _store = store;
        _notifier = notifier;
    }

    /// <summary>
    /// Returns posts after checking thread access.
    /// </summary>
    public List<Post> GetPosts(int userId, int threadId)
    {
        lock (_store.SyncRoot)
        {
            RequireReadAccess(userId, threadId);

            return _store.Posts
                .Where(p => p.ThreadId == threadId)
                .OrderBy(p => p.Id)
                .Select(CopyPost)
                .ToList();
        }
    }

    /// <summary>
    /// Creates a post or reply in a joined thread.
    /// </summary>
    public Post AddPost(
        int userId,
        int threadId,
        string content,
        int? parentPostId = null)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Message cannot be empty.");

        Post result;

        lock (_store.SyncRoot)
        {
            RequireUser(userId);
            var thread = RequireThread(threadId);

            if (!thread.MemberUserIds.Contains(userId))
                throw new UnauthorizedAccessException(
                    "Join the thread before posting.");

            if (parentPostId.HasValue)
            {
                bool parentExists = _store.Posts.Any(p =>
                    p.Id == parentPostId.Value &&
                    p.ThreadId == threadId);

                if (!parentExists)
                    throw new ArgumentException(
                        "The post you are replying to does not exist.");
            }

            var post = new Post
            {
                Id = _store.NextPostId(),
                ThreadId = threadId,
                AuthorUserId = userId,
                Content = content.Trim(),
                CreatedAt = DateTime.UtcNow,
                ParentPostId = parentPostId
            };

            _store.Posts.Add(post);
            result = CopyPost(post);
        }

        _notifier.NotifyChanged();
        return result;
    }

    /// <summary>
    /// Deletes a post when the requester is its author.
    /// </summary>
    public void DeletePost(int userId, int postId)
    {
        lock (_store.SyncRoot)
        {
            RequireUser(userId);

            var post = _store.Posts.FirstOrDefault(p => p.Id == postId)
                ?? throw new KeyNotFoundException("Post not found.");

            if (post.AuthorUserId != userId)
                throw new UnauthorizedAccessException(
                    "You can only delete your own posts.");

            var idsToDelete = new HashSet<int> { postId };

            bool foundMore;

            do
            {
                foundMore = false;

                var replyIds = _store.Posts
                    .Where(p => p.ParentPostId.HasValue &&
                                idsToDelete.Contains(p.ParentPostId.Value))
                    .Select(p => p.Id)
                    .ToList();

                foreach (var replyId in replyIds)
                {
                    if (idsToDelete.Add(replyId))
                        foundMore = true;
                }

            } while (foundMore);

            _store.Posts.RemoveAll(p => idsToDelete.Contains(p.Id));
        }

        _notifier.NotifyChanged();
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

    private void RequireReadAccess(int userId, int threadId)
    {
        var user = RequireUser(userId);
        var thread = RequireThread(threadId);

        if (thread.IsPrivate &&
            !user.IsAuditor &&
            !thread.MemberUserIds.Contains(userId))
        {
            throw new UnauthorizedAccessException(
                "You do not have access to this private thread.");
        }
    }

    private static Post CopyPost(Post post)
    {
        return new Post
        {
            Id = post.Id,
            ThreadId = post.ThreadId,
            AuthorUserId = post.AuthorUserId,
            Content = post.Content,
            CreatedAt = post.CreatedAt,
            ParentPostId = post.ParentPostId
        };
    }
}
