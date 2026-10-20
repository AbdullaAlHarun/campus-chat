
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace CampusChat.Services;

public sealed class UserStateService
{
    private const string StorageKey = "campuschat-user-id";

    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly UserService _userService;

    public int? CurrentUserId { get; private set; }

    public bool IsSignedIn => CurrentUserId.HasValue;

    public event Action? OnChange;

    /// <summary>
    /// Creates the user session service.
    /// </summary>
    public UserStateService(
        ProtectedSessionStorage sessionStorage,
        UserService userService)
    {
        _sessionStorage = sessionStorage;
        _userService = userService;
    }

    /// <summary>
    /// Restores the selected user from browser session storage.
    /// </summary>
    public async Task RestoreSessionAsync()
    {
        try
        {
            var result = await _sessionStorage.GetAsync<int>(StorageKey);

            if (result.Success &&
                _userService.GetUserById(result.Value) != null)
            {
                CurrentUserId = result.Value;
            }
            else
            {
                CurrentUserId = null;
            }
        }
        catch (InvalidOperationException)
        {
            CurrentUserId = null;
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            CurrentUserId = null;
        }

        OnChange?.Invoke();
    }

    /// <summary>
    /// Selects a seeded user and saves the selection.
    /// </summary>
    public async Task SignInAsync(int userId)
    {
        if (_userService.GetUserById(userId) == null)
            throw new ArgumentException("Selected user does not exist.");

        await _sessionStorage.SetAsync(StorageKey, userId);

        CurrentUserId = userId;
        OnChange?.Invoke();
    }

    /// <summary>
    /// Clears the selected user from the browser session.
    /// </summary>
    public async Task SignOutAsync()
    {
        await _sessionStorage.DeleteAsync(StorageKey);

        CurrentUserId = null;
        OnChange?.Invoke();
    }
}
