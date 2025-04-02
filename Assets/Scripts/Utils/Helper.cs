using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;


public static class Helper {
    // public static string DeviceID = SystemInfo.deviceUniqueIdentifier;

    public static Transform FindChildRecursive(Transform parent, string childName) {
        foreach (Transform child in parent) {
            if (child.name == childName)
                return child;

            Transform result = FindChildRecursive(child, childName);
            if (result != null)
                return result;
        }
        return null;
    }


    public static Color ColorFromHex(string hex) {
        Color color;
        if (ColorUtility.TryParseHtmlString(hex, out color)) {
            return color;
        }
        Debug.LogWarning("Invalid hex code: " + hex);
        return Color.white;
    }


    public static object Get<TKey, TValue>(Dictionary<TKey, TValue> dict, TKey key) {
        if (dict.TryGetValue(key, out TValue value)) {
            return value;
        }
        return null;
    }

    public static Dictionary<string, Texture> CacheUrlTextures = new Dictionary<string, Texture> { };
    public static Texture GetTexture(string url) {
        Texture temp = (Texture)Get(CacheUrlTextures, url);
        return temp;
    }

    public static string GetLocalizedValue(string key, object[] args = null) {
        LocalizedString localized = new LocalizedString();
        localized.TableReference = "Game";
        localized.TableEntryReference = key;
        if (args != null) {
            localized.Arguments = args;
        }
        return localized.GetLocalizedString();
    }

    public static string GetLocaleKey() {
        return LocalizationSettings.SelectedLocale.Identifier.Code;
    }

    public static T GetRandomInArr<T>(T[] objects) {
        return objects[UnityEngine.Random.Range(0, objects.Length)];
    }

    public static T? StringToEnum<T>(string value) where T : struct, Enum {
        if (Enum.TryParse(value, true, out T result)) {
            return result;
        }
        return null;
    }


    public static void Shuffle<T>(List<T> list) {
        System.Random rng = new System.Random();
        int n = list.Count;
        for (int i = n - 1; i > 0; i--) {
            int j = rng.Next(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    public static Texture2D SpriteToTexture(Sprite sprite) {
        if (sprite == null) return null;

        Texture2D texture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);

        Color[] pixels = sprite.texture.GetPixels(
            (int)sprite.rect.x,
            (int)sprite.rect.y,
            (int)sprite.rect.width,
            (int)sprite.rect.height
        );

        texture.SetPixels(pixels);
        texture.Apply();

        return texture;
    }

    public enum Layer {
        Environment,
        PlayerBlue,
        PlayerRed,
    }

    public enum Tag {
        Square,
        Item,
        ChoicesBoard,
    }
}
