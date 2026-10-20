
using CampusChat.Models;

namespace CampusChat.Services;

public sealed class DirectChatService
{
    private readonly CampusChatStore _store;
    private readonly ChangeNotifier _notifier;

    /// <summary>
    /// Creates the direct chat service.
    /// </summary>
    public DirectChatService(
        CampusChatStore store,
        ChangeNotifier notifier)
    {
        _store = store;
        _notifier = notifier;
    }

    /// <summary>
    /// Finds or creates a conversation between two users.
    /// </summary>
    public DirectChat GetOrCreateChat(int userId, int otherUserId)
    {
        if (userId == otherUserId)
            throw new ArgumentException(
                "You cannot start a conversation with yourself.");

        DirectChat result;
        bool created = false;

        lock (_store.SyncRoot)
        {
            RequireUser(userId);
            RequireUser(otherUserId);

            var chat = _store.DirectChats.FirstOrDefault(c =>
                (c.User1Id == userId && c.User2Id == otherUserId) ||
                (c.User1Id == otherUserId && c.User2Id == userId));

            if (chat == null)
            {
                chat = new DirectChat
                {
                    Id = _store.NextChatId(),
                    User1Id = userId,
                    User2Id = otherUserId,
                    CreatedAt = DateTime.UtcNow
                };

                _store.DirectChats.Add(chat);
                created = true;
            }

            result = CopyChat(chat);
        }

        if (created)
            _notifier.NotifyChanged();

        return result;
    }

    /// <summary>
    /// Returns conversations visible to a user.
    /// </summary>
    public List<DirectChat> GetChats(int userId)
    {
        lock (_store.SyncRoot)
        {
            var user = RequireUser(userId);

            return _store.DirectChats
                .Where(c =>
                    user.IsAuditor ||
                    c.User1Id == userId ||
                    c.User2Id == userId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(CopyChat)
                .ToList();
        }
    }

    /// <summary>
    /// Returns a conversation after checking access.
    /// </summary>
    public DirectChat GetChat(int userId, int chatId)
    {
        lock (_store.SyncRoot)
        {
            var chat = RequireChatAccess(userId, chatId);
            return CopyChat(chat);
        }
    }

    /// <summary>
    /// Returns messages from an authorized conversation.
    /// </summary>
    public List<DirectMessage> GetMessages(int userId, int chatId)
    {
        lock (_store.SyncRoot)
        {
            RequireChatAccess(userId, chatId);

            return _store.DirectMessages
                .Where(m => m.DirectChatId == chatId)
                .OrderBy(m => m.Id)
                .Select(CopyMessage)
                .ToList();
        }
    }

    /// <summary>
    /// Sends a message as a conversation participant.
    /// </summary>
    public DirectMessage SendMessage(
        int userId,
        int chatId,
        string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Message cannot be empty.");

        DirectMessage result;

        lock (_store.SyncRoot)
        {
            var chat = RequireChatAccess(userId, chatId);

            if (chat.User1Id != userId && chat.User2Id != userId)
                throw new UnauthorizedAccessException(
                    "Only conversation participants can send messages.");

            var message = new DirectMessage
            {
                Id = _store.NextMessageId(),
                DirectChatId = chatId,
                SenderUserId = userId,
                Content = content.Trim(),
                SentAt = DateTime.UtcNow
            };

            _store.DirectMessages.Add(message);
            result = CopyMessage(message);
        }

        _notifier.NotifyChanged();
        return result;
    }

    private User RequireUser(int userId)
    {
        return _store.Users.FirstOrDefault(u => u.Id == userId)
            ?? throw new KeyNotFoundException("User not found.");
    }

    private DirectChat RequireChatAccess(int userId, int chatId)
    {
        var user = RequireUser(userId);

        var chat = _store.DirectChats.FirstOrDefault(c => c.Id == chatId)
            ?? throw new KeyNotFoundException("Conversation not found.");

        if (!user.IsAuditor &&
            chat.User1Id != userId &&
            chat.User2Id != userId)
        {
            throw new UnauthorizedAccessException(
                "You cannot access this conversation.");
        }

        return chat;
    }

    private static DirectChat CopyChat(DirectChat chat)
    {
        return new DirectChat
        {
            Id = chat.Id,
            User1Id = chat.User1Id,
            User2Id = chat.User2Id,
            CreatedAt = chat.CreatedAt
        };
    }

    private static DirectMessage CopyMessage(DirectMessage message)
    {
        return new DirectMessage
        {
            Id = message.Id,
            DirectChatId = message.DirectChatId,
            SenderUserId = message.SenderUserId,
            Content = message.Content,
            SentAt = message.SentAt
        };
    }
}
