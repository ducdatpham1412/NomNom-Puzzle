using UnityEditor;
using UnityEngine;

public class Utilities {
    [MenuItem("Tools/Clear PlayerPrefs")]
    public static void ClearPlayerPrefs() {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("PlayerPrefs cleared.");
    }
}

