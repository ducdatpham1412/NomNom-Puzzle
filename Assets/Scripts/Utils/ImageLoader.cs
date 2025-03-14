using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ImageLoader : MonoBehaviour {
    public string ImageUrl = "";


    void Awake() {
        if (ImageUrl.Length > 0) {
            StartCoroutine(DownloadImage(ImageUrl));
        }
    }

    public void SetImageUrl(string url) {
        ImageUrl = url;
        StartCoroutine(DownloadImage(ImageUrl));
    }

    private IEnumerator DownloadImage(string url) {
        if (url == null) {
            yield break;
        }
        if (Helper.CacheUrlTextures.ContainsKey(url)) {
            GetComponent<RawImage>().texture = Helper.GetTexture(url);
            yield break;
        }

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success) {
            Debug.LogWarning($"Failed to load image from URL {url}: {request.error}");
        }
        else {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            if (!Helper.CacheUrlTextures.ContainsKey(url)) {
                Helper.CacheUrlTextures.Add(url, texture);
            }
            GetComponent<RawImage>().texture = texture;
        }
    }
}
