using UnityEngine;

public class ScaleOnEnable : MonoBehaviour {
    [SerializeField] LeanTweenType tweenType = LeanTweenType.easeSpring;
    [SerializeField] float duration = 0.5f;
    Vector3 originalScale;

    void Awake() {
        var rt = GetComponent<RectTransform>();
        originalScale = rt.localScale;
        rt.localScale = Vector3.zero;
    }

    void OnEnable() {
        LeanTween.scale(gameObject, originalScale, duration).setEase(tweenType);
    }

    void OnDisable() {
        LeanTween.scale(gameObject, Vector3.zero, duration).setEase(tweenType);
    }
}
