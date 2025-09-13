using UnityEngine;

public static class GameInputManager
{
    private static bool isPaused = false;
    
    public static bool IsPaused => isPaused;
    
    public static void SetPaused(bool paused)
    {
        isPaused = paused;
        Debug.Log($"🎮 Input Manager: Game paused = {paused}");
        
        // Also set timescale for visual feedback
        Time.timeScale = paused ? 0f : 1f;
    }
}
