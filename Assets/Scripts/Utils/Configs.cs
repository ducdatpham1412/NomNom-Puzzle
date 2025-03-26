using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class Configs {
    public static _Color Color = new _Color {
        brown = "#4E3200",
        green = "#007427",
        green01 = "#0BF500",
        golden = "#FFEEA3",
        silver = "#D1D1D1",
        bronze = "#FFC393",
        yellow = "#FFCE45",
    };

    public static _LocaleID LocaleID = new _LocaleID {
        En = "en",
        Jp = "jp",
        Ko = "ko",
        Vi = "vi",
    };

    public static _Env Env = new _Env {
        match_id = GetEnv("match_id"),
        match_port = GetEnv("match_port"),
        api_url = GetEnv("API_URL"),
        socket_url = GetEnv("SOCKET_URL"),
    };

    public static Color RootSquareColor = Helper.ColorFromHex("#B5B5B5");
    public static List<Color> SquareColors = new List<Color>{
        Helper.ColorFromHex("#78EE4B"),
        Helper.ColorFromHex("#E77735"),
        Helper.ColorFromHex("#2EC30E"),
        Helper.ColorFromHex("#B735E7"),
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
        string basePath = Directory.GetCurrentDirectory();
        if (Application.isEditor) {
            basePath = basePath.Split("/NomNom")[0] + "/NomNom";
        }

        string filePath = Path.Combine(basePath, ".env");

        if (!File.Exists(filePath)) {
            Debug.Log($"File path does not existed: {filePath}");
            return;
        }

        foreach (var line in File.ReadAllLines(filePath)) {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                continue;

            var parts = line.Split('=', 2);
            if (parts.Length != 2)
                continue;

            var key = parts[0].Trim();
            var value = parts[1].Trim();
            Debug.Log("Set env: " + key + " - " + value);
            Environment.SetEnvironmentVariable(key, value);
        }
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
        public string match_id;
        public string match_port;
        public string api_url;
        public string socket_url;
    }
}
