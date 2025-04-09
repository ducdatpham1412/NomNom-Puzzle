using Newtonsoft.Json;
using UnityEngine;

public static class Storage {
    public enum Key {
        account, // AppState.Account
        currentLevel,
        playingLevels,
        profile,
    }

    public static void SET(Key key, string value) {
        PlayerPrefs.SetString(key.ToString(), value);
        PlayerPrefs.Save();
    }

    public static T? GETStruct<T>(Key key) where T : struct {
        string res = PlayerPrefs.GetString(key.ToString());
        if (string.IsNullOrEmpty(res)) return null;

        try {
            return JsonConvert.DeserializeObject<T>(res);
        }
        catch (JsonException jsonEx) {
            Debug.LogWarning($"Deserialization failed: {jsonEx.Message}");
            return null;
        }
    }

    public static T GETRef<T>(Key key) where T : class {
        string res = PlayerPrefs.GetString(key.ToString());
        if (string.IsNullOrEmpty(res)) return null;

        try {
            return JsonConvert.DeserializeObject<T>(res);
        }
        catch (JsonException jsonEx) {
            Debug.LogWarning($"Deserialization failed: {jsonEx.Message}");
            return null;
        }
    }

    public static void DELETE(Key key) {
        PlayerPrefs.DeleteKey(key.ToString());
        PlayerPrefs.Save();
    }
}
