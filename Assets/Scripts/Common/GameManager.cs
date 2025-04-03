using System;
using UnityEngine;

public class GameManager : Singleton<GameManager> {
    protected GameManager() { }

    public GameState gameState = new GameState();
    public event Action<GameState> OnGameStateChanged;
    public GameController Controller;

    public void Initialize() { }

    void OnApplicationQuit() {
        // TODO: Save game status
    }

    void OnApplicationPause(bool pauseStatus) {
        // TODO: Check "in_game" status to go to match scene
        if (pauseStatus) {
            Debug.Log("App is paused (background mode)");
        }
    }

    public void UpdateGameState(GameState state) {
        gameState = state;
        OnGameStateChanged?.Invoke(gameState);
    }

    public GameState UpdateGameState(Func<GameState, GameState> action) {
        gameState = action(gameState);
        OnGameStateChanged?.Invoke(gameState);
        return gameState;
    }
}

