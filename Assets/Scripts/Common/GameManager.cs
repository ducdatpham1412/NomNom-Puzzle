using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class GameManager : Singleton<GameManager> {
    protected GameManager() { }

    public GameState gameState;
    public GameController Controller;

    void Awake() {
        List<GameState.PlayingLevel> playingLevels = Storage.GETRef<List<GameState.PlayingLevel>>(Storage.Key.playingLevels);
        playingLevels = playingLevels ?? new List<GameState.PlayingLevel>();
        gameState = new GameState {
            playingLevels = playingLevels,
        };
    }

    void Start() {
        if (Application.isEditor) {
            Application.targetFrameRate = 30;
        }
    }

    void OnApplicationQuit() {
        string playingLevels = JsonConvert.SerializeObject(gameState.playingLevels);
        Storage.SET(Storage.Key.playingLevels, playingLevels);
    }

    void OnApplicationPause(bool pauseStatus) {
        // TODO: Check "in_game" status to go to match scene
        if (pauseStatus) {
            Debug.Log("App is paused (background mode)");
        }
    }

    public void Initialize() { }
}

