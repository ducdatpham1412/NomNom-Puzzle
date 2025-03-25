using Newtonsoft.Json;
using UnityEngine;

public static class Storage {
    public enum Key {
        account, // AppState.Account
        profile, // AppState.Profile
    }

    public static void SET(Key key, string value) {
        PlayerPrefs.SetString(key.ToString(), value);
        PlayerPrefs.Save();
    }

    public static T GET<T>(Key key) {
        string res = PlayerPrefs.GetString(key.ToString());

        if (string.IsNullOrEmpty(res)) {
            return default(T);
        }
        try {
            return JsonConvert.DeserializeObject<T>(res);
        }
        catch (JsonException jsonEx) {
            Debug.LogWarning($"Deserialization failed: {jsonEx.Message}");
            return default(T);
        }
    }
}
