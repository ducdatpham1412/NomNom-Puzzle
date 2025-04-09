using UnityEngine;
using UnityEngine.UI;

public class CanvasBackground : MonoBehaviour {
    [SerializeField] Image Background;

    void Start() {
        Background.sprite = GameManager.Instance.GetBackground();
    }
}
