using UnityEngine;

public class BackgroundHover : MonoBehaviour {
    void Start() {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = GameManager.Instance.background;
        renderer.color = Helper.ColorFromHex("#C5C5C5");
    }
}
