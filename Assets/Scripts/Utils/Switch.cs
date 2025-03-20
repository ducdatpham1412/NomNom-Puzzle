using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Switch : MonoBehaviour, IPointerClickHandler {
    [SerializeField] Image Fill;
    [SerializeField] Slider Slider;
    public Action<bool> ActionChange;

    Coroutine sc;

    public void SetValue(bool isActive) {
        Slider.value = isActive ? 1 : 0;
    }

    public void OnValueChanged(float value) {
        Color c = Fill.color;
        Fill.color = new Color(c.r, c.g, c.b, value);
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (sc != null) return;
        sc = StartCoroutine(SwitchCoroutine());
    }

    IEnumerator SwitchCoroutine() {
        float current = Slider.value;
        bool shouldOpen = current == 0;
        float target = shouldOpen ? 1f : 0f;
        float duration = 0.24f;
        float elapsedTime = 0f;
        while (elapsedTime < duration) {
            Slider.value = Mathf.Lerp(current, target, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Slider.value = target;
        sc = null;
        ActionChange?.Invoke(shouldOpen);
    }
}
