using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;


public class Configs {
    public static readonly _Color Color = new _Color {
        brown = "#4E3200",
        green = "#007427",
        green01 = "#0BF500",
        golden = "#FFEEA3",
        silver = "#D1D1D1",
        bronze = "#FFC393",
        yellow = "#FFCE45",
    };

    public static readonly _LocaleID LocaleID = new _LocaleID {
        En = "en",
        Jp = "jp",
        Ko = "ko",
        Vi = "vi",
    };

    public static readonly _Env Env = new _Env {
        API_URL = GetEnv("API_URL"),
        BANNER_ID = GetAdModID(android: "BANNER_ANDROID_ID", ios: "BANNER_IOS_ID"),
        REWARD_ID = GetAdModID(android: "REWARD_ANDROID_ID", ios: "REWARD_IOS_ID"),
        INTERSTITIAL_ID = GetAdModID(android: "INTERSTITIAL_ANDROID_ID", ios: "INTERSTITIAL_IOS_ID"),
    };

    public static readonly Color RootSquareColor = Helper.ColorFromHex("#B5B5B5");
    public static readonly Color DefaultSquareColor = Helper.ColorFromHex("#AC680F");
    public static readonly Color ErrorSquareColor = Helper.ColorFromHex("#FF0000");
    public static readonly List<Color> SquareColors = new List<Color>{
        Helper.ColorFromHex("#2D9502"),
        Helper.ColorFromHex("#EAD425"),
        Helper.ColorFromHex("#05C5C8"),
        Helper.ColorFromHex("#164EF1"),
    };

    static bool hasReadEnv = false;

    static string GetEnv(string key) {
        if (!hasReadEnv) {
            ReadEnvFile();
            hasReadEnv = true;
        }

        try {
            string value = Environment.GetEnvironmentVariable(key);
            return value;
        }
        catch (Exception) {
            Debug.LogWarning($"No env found: {key}");
            return null;
        }
    }

    static void ReadEnvFile() {
        TextAsset envText = Resources.Load<TextAsset>("env");
        var envDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(envText.text);
        foreach (var (key, value) in envDict) {
            Environment.SetEnvironmentVariable(key, value);
        }
    }


    static string GetAdModID(string android, string ios) {
#if UNITY_ANDROID
        return GetEnv(android);
#elif UNITY_IOS
    return GetEnv(ios);
#else
    return "invalid";
#endif
    }


    [Serializable]
    public class _Color {
        public string brown;
        public string green;
        public string green01;
        public string golden;
        public string silver;
        public string bronze;
        public string yellow;
    }

    [Serializable]
    public class _LocaleID {
        public string En;
        public string Jp;
        public string Ko;
        public string Vi;
    }

    [Serializable]
    public class _Env {
        public string API_URL;
        public string BANNER_ID;
        public string REWARD_ID;
        public string INTERSTITIAL_ID;
    }
}
