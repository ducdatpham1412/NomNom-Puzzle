using System;
using System.Collections.Generic;
using UnityEngine;


public class WelcomeLogic : MonoBehaviour {
    public List<CanvasEntry> canvases = new List<CanvasEntry>();
    public GameObject CreateAccountError;

    bool shouldAddNewCanvas = true;
    List<string> histories = new List<string>();

    void Start() {
        GameManager.Instance.Initialize();
        SoundManager.Instance.PlayMusic(SoundManager.MusicSource.background);
        InitApp();
    }

    private async void InitApp() {
        try {
            var storageProfile = Storage.GET<Profile>(Storage.Key.account);

            if (storageProfile != null) {
                GameManager.Instance.appState.resource = await ApiManager.GET<Resource>("/common/resource");
                var passport = await ApiManager.GET<Passport>("/common/passport");
                GameManager.Instance.appState.profile = passport.profile;
                Storage.SetProfile(passport.profile);
                LocalizationManager.Instance.SetLocale(passport.profile.setting.language == "en" ? 0 : 1);
                Navigator.Instance.NavigateTo(Navigator.Scene.Home);
                return;
            }

            ShowCanvas("SelectLanguage");
        }
        catch (Exception ex) {
            Debug.Log($"Error: {ex.Message}");
        }
    }

    public void ShowCanvas(string canvasName) {
        if (shouldAddNewCanvas) {
            histories.Add(canvasName);
        }
        else {
            shouldAddNewCanvas = true;
        }
        foreach (CanvasEntry cv in canvases) {
            if (cv.name.ToString() == canvasName) {
                cv.gameObject.SetActive(true);
                // currentCanvas = cv.name;
            }
            else {
                cv.gameObject.SetActive(false);
            }
        }
    }

    public void GoBack() {
        if (histories.Count >= 2) {
            shouldAddNewCanvas = false;
            histories.RemoveAt(histories.Count - 1);
            ShowCanvas(histories[histories.Count - 1]);
        }
    }

    public void PauseUnPauseMusicBackground() {
        SoundManager.Instance.PauseUnPauseMusicBackground();
    }

    public void SetLocale(int localeID) {
        LocalizationManager.Instance.SetLocale(localeID);
    }

    public enum CanvasName {
        SelectLanguage,
        LoginCreateAccount,
        Login,
        CreateAccount,
        EnterName,
    }


    public class CanvasEntry {
        public CanvasName name;
        public GameObject gameObject;
    }

}
