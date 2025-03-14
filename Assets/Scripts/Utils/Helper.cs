using System;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;


public static class Helper {
    public static string DeviceID = SystemInfo.deviceUniqueIdentifier;

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


    public static void SetImageUrl(Transform gameObject, string url) {
        ImageLoader imageLoader = gameObject.GetComponent<ImageLoader>();
        imageLoader.SetImageUrl(url);
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


    public static long TsNow() {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public static string GetID(int byteLength = 5) {
        byte[] randomBytes = new byte[byteLength];
        using (var rng = RandomNumberGenerator.Create()) {
            rng.GetBytes(randomBytes);
        }
        string base64String = Convert.ToBase64String(randomBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
        return base64String;
    }

    public static Dictionary<string, Texture> CacheUrlTextures = new Dictionary<string, Texture> { };
    public static Texture GetTexture(string url) {
        Texture temp = (Texture)Get(CacheUrlTextures, url);
        return temp;
    }

    public static string GetLocalizedValue(LocalizationManager.Table table, string key, object[] args = null) {
        LocalizedString localized = new LocalizedString();
        localized.TableReference = table.ToString();
        localized.TableEntryReference = key;
        if (args != null) {
            localized.Arguments = args;
        }
        return localized.GetLocalizedString();
    }

    public static string GetLocaleKey() {
        return LocalizationSettings.SelectedLocale.Identifier.Code;
    }

    public static async Task<Sprite> ImgUrlToSprite(string url) {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        var asyncOperation = request.SendWebRequest();

        while (!asyncOperation.isDone) {
            await Task.Yield(); // Yield control back to the Unity main thread
        }

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError) {
            Debug.LogWarning(request.error);
            return null;
        }
        else {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
    }

    public static void FitSpriteToGameObject(GameObject targetObject) {
        SpriteRenderer spriteRenderer = targetObject.GetComponent<SpriteRenderer>();

        Bounds spriteBounds = spriteRenderer.sprite.bounds;
        Vector3 spriteSize = spriteBounds.size;

        Vector3 targetScale = targetObject.transform.localScale;

        float xScale = targetScale.x / spriteSize.x;
        float yScale = targetScale.y / spriteSize.y;

        float scaleFactor = Mathf.Min(xScale, yScale);

        targetObject.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
    }

    public static string RenderErrorMsg(string error_msg) {
        return error_msg;
    }

    public static long TimeStamp() {
        return DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }

    public static T GetRandomInArr<T>(T[] objects) {
        return objects[UnityEngine.Random.Range(0, objects.Length)];
    }

    public enum Layer {
        Environment,
        PlayerBlue,
        PlayerRed,
    }

    public enum Tag {
        Player,
        VFX,
        Grass,
        Obstacle,
        Item,
        Bullet,
    }
}
