using System;

public static class SessionEvents
{
    // This is our "Message." Anyone can subscribe to it or trigger it.
    public static event Action OnStartGameRequested;

    public static void TriggerStartGame()
    {
        OnStartGameRequested?.Invoke();
    }
}