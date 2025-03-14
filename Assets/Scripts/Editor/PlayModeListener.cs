#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class PlayModeListener {
    static PlayModeListener() {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state) {
        if (state == PlayModeStateChange.ExitingPlayMode) {
            Debug.Log("Play mode is stopping. Cleanup or save operations can be done here.");
            // SocketManager.Disconnected();
        }
    }
}
#endif
