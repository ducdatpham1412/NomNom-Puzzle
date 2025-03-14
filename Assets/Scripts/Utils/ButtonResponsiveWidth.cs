using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ButtonResponsiveWidth : MonoBehaviour {
    public float maxWidth = 200f;

    void Start() {
        AdjustWidth();
    }

    private void AdjustWidth() {
        RectTransform rectTransform = GetComponent<RectTransform>();
        float canvasWidth = Screen.width;

        float targetWidth = Mathf.Min(canvasWidth * 0.5f, maxWidth);

        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
    }
}
