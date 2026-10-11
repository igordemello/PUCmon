using System.Runtime.InteropServices;

/// <summary>The web page around the game (WebGLTemplates/FreeWebAR).</summary>
public static class Page
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] static extern void PUCmon_PageReady();
#endif

    /// <summary>
    /// Takes the page's loading screen away. Each scene calls it once it knows what to show, so a player whose session
    /// is still being checked never sees (or taps) a screen that is about to change.
    /// </summary>
    public static void Ready()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        PUCmon_PageReady();
#endif
    }
}
