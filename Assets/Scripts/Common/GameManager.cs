using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class GameManager : Singleton<GameManager> {
    protected GameManager() { }

    public GameState gameState;
    public Profile profile;
    public GameController Controller;

    Sprite background;

    void Awake() {
        List<GameState.PlayingLevel> playingLevels = Storage.GETRef<List<GameState.PlayingLevel>>(Storage.Key.playingLevels);
        playingLevels = playingLevels ?? new List<GameState.PlayingLevel>();
        gameState = new GameState {
            playingLevels = playingLevels,
        };

        profile = Storage.GETRef<Profile>(Storage.Key.profile);
        profile = profile ?? new Profile {
            device_id = SystemInfo.deviceUniqueIdentifier,
            localeID = null,
            music = true,
            sfx = true,
        };

        SoundManager.Instance.Initialize();
        if (profile.music) {
            SoundManager.Instance.PlayMusic(SoundManager.MusicSource.Kid);
        }
        if (profile.localeID != null) {
            LocalizationManager.Instance.SetLocale((int)profile.localeID);
        }
    }

    void Start() {
        if (Application.isEditor) {
            Application.targetFrameRate = 30;
        }
        GoogleAds.Instance.ShowBanner();
    }

    void OnApplicationQuit() {
        OnQuit();
    }

    void OnApplicationPause(bool pauseStatus) {
        if (pauseStatus) {
            Debug.Log("App is paused (background mode)");
        }
    }

    public void Initialize() { }

    public void OnQuit() {
        Controller.SaveLevelStatus();
        string playingLevels = JsonConvert.SerializeObject(gameState.playingLevels);
        string strProfile = JsonConvert.SerializeObject(profile);
        Storage.SET(Storage.Key.playingLevels, playingLevels);
        Storage.SET(Storage.Key.profile, strProfile);
    }

    public Sprite GetBackground() {
        if (background != null) return background;
        Sprite[] bgSprites = Resources.LoadAll<Sprite>("Images");
        background = bgSprites[Random.Range(0, bgSprites.Length)];
        return background;
    }
}

