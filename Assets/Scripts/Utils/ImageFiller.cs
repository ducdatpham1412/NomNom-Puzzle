using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ImageFiller : MonoBehaviour {
    private RectTransform rectTransform;
    private Image image;

    void Awake() {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();

        if (image.sprite == null) return;

        float imageRatio = (float)image.sprite.texture.width / image.sprite.texture.height;
        float panelRatio = rectTransform.rect.width / rectTransform.rect.height;

        if (imageRatio > panelRatio) {
            image.rectTransform.sizeDelta = new Vector2(rectTransform.rect.height * imageRatio, rectTransform.rect.height);
        }
        else {
            image.rectTransform.sizeDelta = new Vector2(rectTransform.rect.width, rectTransform.rect.width / imageRatio);
        }
    }
}
