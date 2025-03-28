using Newtonsoft.Json;
using UnityEngine;

public static class Storage {
    public enum Key {
        account, // AppState.Account
        profile, // AppState.Profile
        currentLevel,
    }

    public static void SET(Key key, string value) {
        PlayerPrefs.SetString(key.ToString(), value);
        PlayerPrefs.Save();
    }

    public static T? GET<T>(Key key) where T : struct {
        string res = PlayerPrefs.GetString(key.ToString());

        if (string.IsNullOrEmpty(res)) {
            return null;
        }
        try {
            return JsonConvert.DeserializeObject<T>(res);
        }
        catch (JsonException jsonEx) {
            Debug.LogWarning($"Deserialization failed: {jsonEx.Message}");
            return null;
        }
    }
}
