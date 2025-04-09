using UnityEngine;

public class WelcomeLogic : MonoBehaviour {
    [SerializeField] ButtonSound BtnSound;
    [SerializeField] Canvas SelectLanguage;

    void Start() {
        GameManager.Instance.Initialize();
        if (GameManager.Instance.profile.localeID == null) {
            BtnSound.SetPlaying(SoundManager.Instance.Music.isPlaying);
            SelectLanguage.gameObject.SetActive(true);
        }
        else {
            Navigator.Instance.NavigateTo(Navigator.Scene.GameScene);
        }
    }

    public void PauseUnPauseMusicBackground() {
        SoundManager.Instance.PauseUnPauseMusicBackground();
        bool isPlaying = SoundManager.Instance.Music.isPlaying;
        BtnSound.SetPlaying(isPlaying);
        GameManager.Instance.profile.music = isPlaying;
    }

    public void SetLocale(int localeID) {
        LocalizationManager.Instance.SetLocale(localeID, callback: () => {
            GameManager.Instance.profile.localeID = localeID;
            Navigator.Instance.NavigateTo(Navigator.Scene.GameScene);
        });
    }
}
