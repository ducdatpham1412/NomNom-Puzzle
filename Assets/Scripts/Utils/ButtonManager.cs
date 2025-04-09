using System;
using UnityEngine;
using UnityEngine.UI;


public class ButtonManager : MonoBehaviour {
    [Header("GameObjects")]
    [SerializeField] GameObject Title;
    [SerializeField] LoadingManager loadingManager;

    [Header("Stats")]
    [SerializeField] SoundManager.SF audioClick = SoundManager.SF.Pop_01;
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
        SoundManager.Instance.PlaySF(audioClick);

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
