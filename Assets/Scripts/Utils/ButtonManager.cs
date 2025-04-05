using System;
using UnityEngine;
using UnityEngine.UI;


public enum AudioClick {
    KnockWood,
    None,
}

public class ButtonManager : MonoBehaviour {
    [Header("GameObjects")]
    [SerializeField] GameObject Title;
    [SerializeField] LoadingManager loadingManager;

    [Header("Stats")]
    [SerializeField] AudioClick audioClick = AudioClick.KnockWood;
    [SerializeField] LeanTweenType animationClick = LeanTweenType.punch;

    public Action OnEndAnimationClick;

    Button button;
    Vector3 originalScale;
    RectTransform rectTransform;

    void Start() {
        button = GetComponent<Button>();
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
        button.onClick.AddListener(OnClick);
    }

    void OnClick() {
        if (audioClick == AudioClick.KnockWood) {
            SoundManager.Instance.PlaySF(SoundManager.SF.KnockWood);
        }

        if (animationClick != LeanTweenType.notUsed) {
            LeanTween.cancel(gameObject);
            rectTransform.localScale = originalScale;
            LeanTween.scale(gameObject, originalScale * 1.2f, 1f).setEase(animationClick).setOnComplete(() => {
                if (animationClick != LeanTweenType.punch) {
                    LeanTween.scale(gameObject, originalScale, 1f).setEase(animationClick);
                }
                OnEndAnimationClick?.Invoke();
            });
        }
    }

    public void Disable() {
        if (button != null) {
            button.interactable = false;
        }
    }

    public void Enable() {
        if (button != null) {
            button.interactable = true;
        }
    }

    public void StartLoading() {
        Disable();
        if (loadingManager != null) {
            Title.SetActive(false);
            loadingManager.StartLoading();
        }
    }
    public void StopLoading() {
        Enable();
        if (loadingManager != null && Title != null) {
            Title.SetActive(true);
            loadingManager.StopLoading();
        }
    }
}
