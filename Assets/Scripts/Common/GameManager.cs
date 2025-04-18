using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using Unity.Notifications.Android;
using System;

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
        ScheduleDailyNotification();

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
    }

    void OnApplicationQuit() {
        OnQuit();
    }

    void OnApplicationPause(bool pauseStatus) {
        if (pauseStatus) {
            OnQuit();
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
        Sprite[] bgSprites = Resources.LoadAll<Sprite>("Images/Background");
        background = bgSprites[UnityEngine.Random.Range(0, bgSprites.Length)];
        return background;
    }

    public void RescheduleDailyNotification() {
        int? notiID = Storage.GETStruct<int>(Storage.Key.dailyNoti);
        if (notiID != null) {
            AndroidNotificationCenter.CancelNotification((int)notiID);
            Storage.DELETE(Storage.Key.dailyNoti);
        }
        ScheduleDailyNotification();
    }

    void ScheduleDailyNotification() {
        if (Storage.GETStruct<int>(Storage.Key.dailyNoti) != null) return;

        var channel = new AndroidNotificationChannel() {
            Id = "daily_reminder",
            Name = "Daily Reminder",
            Importance = Importance.Default,
            Description = "Daily reminder notification",
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);

        var notification = new AndroidNotification {
            Title = "Nom Nom Nom 🧠 🧩",
            Text = Helper.GetLocalizedValue("timeToTrainBrain"),
            FireTime = DateTime.Today.AddHours(20),
            RepeatInterval = TimeSpan.FromDays(1),
        };

        if (notification.FireTime <= DateTime.Now)
            notification.FireTime = notification.FireTime.AddDays(1);

        int notiID = AndroidNotificationCenter.SendNotification(notification, "daily_reminder");

        Storage.SET(Storage.Key.dailyNoti, notiID.ToString());
    }
}

