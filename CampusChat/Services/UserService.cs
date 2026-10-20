
using CampusChat.Models;

namespace CampusChat.Services;

public sealed class UserService
{
    private readonly CampusChatStore _store;
    private readonly ChangeNotifier _notifier;

    /// <summary>
    /// Creates a user service with shared data and notifications.
    /// </summary>
    public UserService(
        CampusChatStore store,
        ChangeNotifier notifier)
    {
        _store = store;
        _notifier = notifier;
    }

    /// <summary>
    /// Returns all user profiles.
    /// </summary>
    public List<User> GetUsers()
    {
        lock (_store.SyncRoot)
        {
            return _store.Users.Select(CopyUser).ToList();
        }
    }

    /// <summary>
    /// Finds a user by ID.
    /// </summary>
    public User? GetUserById(int userId)
    {
        lock (_store.SyncRoot)
        {
            var user = _store.Users.FirstOrDefault(u => u.Id == userId);

            return user == null ? null : CopyUser(user);
        }
    }

    /// <summary>
    /// Updates a user's own profile.
    /// </summary>
    public void UpdateProfile(
        int currentUserId,
        int profileUserId,
        string displayName,
        string bio,
        string avatarColor,
        int studyYear)
    {
        if (currentUserId != profileUserId)
            throw new UnauthorizedAccessException(
                "You can only edit your own profile.");

        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name cannot be empty.");

        if (studyYear < 0)
            throw new ArgumentException("Study year cannot be negative.");

        lock (_store.SyncRoot)
        {
            var user = _store.Users.FirstOrDefault(
                u => u.Id == profileUserId);

            if (user == null)
                throw new KeyNotFoundException("User not found.");

            user.DisplayName = displayName.Trim();
            user.Bio = bio?.Trim() ?? string.Empty;
            user.AvatarColor = string.IsNullOrWhiteSpace(avatarColor)
                ? "#3B82F6"
                : avatarColor.Trim();
            user.StudyYear = studyYear;
        }

        _notifier.NotifyChanged();
    }

    private static User CopyUser(User user)
    {
        return new User
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            Bio = user.Bio,
            AvatarColor = user.AvatarColor,
            StudyYear = user.StudyYear,
            IsAuditor = user.IsAuditor
        };
    }
}
