namespace CampusChat.Services;

public sealed class ChangeNotifier
{
    public event Action? OnChange;

    /// <summary>
    /// Notifies subscribers that application data has changed.
    /// </summary>
    public void NotifyChanged()
    {
        OnChange?.Invoke();
    }
}