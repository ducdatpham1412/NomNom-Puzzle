using UnityEngine;
using UnityEngine.UI;


public enum AudioClick {
    KnockWood,
    None,
}

public class ButtonManager : MonoBehaviour {
    public GameObject Title;
    private Button button;
    public AudioClick audioClick = AudioClick.KnockWood;
    public LoadingManager loadingManager;


    void Start() {
        button = gameObject.GetComponent<Button>();
        if (audioClick != AudioClick.None) {
            button.onClick.AddListener(PlaySound);
        }
    }

    private void PlaySound() {
        if (audioClick == AudioClick.KnockWood) {
            SoundManager.Instance.PlaySF(SoundManager.SF.KnockWood);
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
