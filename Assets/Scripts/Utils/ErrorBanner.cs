using TMPro;
using UnityEngine;

public class ErrorBanner : MonoBehaviour {
    public TMP_Text Content;

    public void Show(string text) {
        Content.text = text;
        gameObject.SetActive(true);
    }

    public void Hide() {
        gameObject.SetActive(false);
    }
}
