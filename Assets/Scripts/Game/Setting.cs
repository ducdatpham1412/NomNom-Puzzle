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
        MusicSwitch.SetValue(true);
        SfxSwitch.SetValue(true);
    }

    void OnDestroy() {
        MusicSwitch.ActionChange -= MusicChanged;
        SfxSwitch.ActionChange -= SfxChanged;
    }

    public void SetLocale(int localeID) {
        LocalizationManager.Instance.SetLocale(localeID);
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
        SoundManager.Instance.PauseUnPauseMusicBackground();
    }

    void SfxChanged(bool isActive) {
        SoundManager.Instance.playSF = isActive;
    }

    [Serializable]
    public class LanguageButton {
        public int Id;
        public Image Image;
    }
}
