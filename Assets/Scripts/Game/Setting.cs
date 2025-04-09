using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Setting : MonoBehaviour {
    [SerializeField] Switch MusicSwitch;
    [SerializeField] Switch SfxSwitch;
    [SerializeField] List<LanguageButton> LanguageButtons = new List<LanguageButton>();

    void Start() {
        MusicSwitch.ActionChange += MusicChanged;
        SfxSwitch.ActionChange += SfxChanged;
        MusicSwitch.SetValue(GameManager.Instance.profile.music);
        SfxSwitch.SetValue(GameManager.Instance.profile.sfx);
        SetLocale(GameManager.Instance.profile.localeID ?? 0);
    }

    void OnDestroy() {
        MusicSwitch.ActionChange -= MusicChanged;
        SfxSwitch.ActionChange -= SfxChanged;
    }

    public void SetLocale(int localeID) {
        Helper.Haptic();
        LocalizationManager.Instance.SetLocale(localeID);
        GameManager.Instance.profile.localeID = localeID;
        foreach (var lan in LanguageButtons) {
            if (lan.Id == localeID) {
                lan.Image.color = Helper.ColorFromHex(Configs.Color.yellow);
            }
            else {
                lan.Image.color = Color.white;
            }
        }
    }

    void MusicChanged(bool isActive) {
        Helper.Haptic();
        GameManager.Instance.profile.music = isActive;
        SoundManager.Instance.PauseUnPauseMusicBackground();
    }

    void SfxChanged(bool isActive) {
        Helper.Haptic();
        GameManager.Instance.profile.sfx = isActive;
    }

    [Serializable]
    public class LanguageButton {
        public int Id;
        public Image Image;
    }
}
