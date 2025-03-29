using UnityEditor;
using UnityEngine;

public class Utilities {
    [MenuItem("Tools/Get Current Level")]
    public static void GetPlayerRef() {
        int? c = Storage.GET<int>(Storage.Key.currentLevel);
        Debug.Log($"C is: {c}");
    }

    [MenuItem("Tools/Set Current Level")]
    public static void SetPlayerRef() {
        Storage.SET(Storage.Key.currentLevel, "1");
    }
}

