
using CampusChat.Models;

namespace CampusChat.Services;

public sealed class UserService
{
    private readonly CampusChatStore _store;
    private readonly ChangeNotifier _notifier;

    /// <summary>Creates the user service.</summary>
    public UserService(
        CampusChatStore store,
        ChangeNotifier notifier)
    {
        _store = store;
        _notifier = notifier;
    }

    /// <summary>Returns all user profiles.</summary>
    public List<User> GetUsers()
    {
        lock (_store.SyncRoot)
        {
            return _store.Users.Select(CopyUser).ToList();
        }
    }

    /// <summary>Finds a user by ID, or returns null.</summary>
    public User? GetUserById(int userId)
    {
        lock (_store.SyncRoot)
        {
            var user = _store.Users.FirstOrDefault(
                u => u.Id == userId);

            return user == null ? null : CopyUser(user);
        }
    }

    /// <summary>Creates a new student profile.</summary>
    public User CreateProfile(
        string displayName,
        string bio,
        string avatarColor,
        int studyYear)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException(
                "Display name cannot be empty.");

        if (studyYear < 1)
            throw new ArgumentException(
                "Study year must be at least 1.");

        User createdUser;

        lock (_store.SyncRoot)
        {
            int nextId = _store.Users.Count == 0
                ? 1
                : _store.Users.Max(u => u.Id) + 1;

            createdUser = new User
            {
                Id = nextId,
                DisplayName = displayName.Trim(),
                Bio = bio?.Trim() ?? string.Empty,
                AvatarColor = string.IsNullOrWhiteSpace(avatarColor)
                    ? "#35745F"
                    : avatarColor.Trim(),
                StudyYear = studyYear,
                IsAuditor = false
            };

            _store.Users.Add(createdUser);
        }

        _notifier.NotifyChanged();

        return CopyUser(createdUser);
    }

    /// <summary>Updates a user's own profile.</summary>
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
            throw new ArgumentException(
                "Display name cannot be empty.");

        if (studyYear < 0)
            throw new ArgumentException(
                "Study year cannot be negative.");

        lock (_store.SyncRoot)
        {
            var user = _store.Users.FirstOrDefault(
                u => u.Id == profileUserId);

            if (user == null)
                throw new KeyNotFoundException(
                    "User not found.");

            user.DisplayName = displayName.Trim();
            user.Bio = bio?.Trim() ?? string.Empty;
            user.AvatarColor = string.IsNullOrWhiteSpace(avatarColor)
                ? "#35745F"
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
