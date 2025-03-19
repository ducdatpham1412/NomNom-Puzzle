using System;
using System.Collections;
using System.Threading;
using UnityEngine;

public class GameManager : Singleton<GameManager> {
    protected GameManager() { }

    public Sprite background;
    SpriteRenderer spriteRenderer;
    SynchronizationContext context;


    // AppState
    public AppState appState = new AppState {
        profile = new Profile(),
        resource = new Resource(),
        account = new Account(),
        client = new ClientValue(),
    };
    public event Action<AppState> OnAppStateChanged;


    // GameState
    public GameState gameState = new GameState();
    public event Action<GameState> OnGameStateChanged;


    // Function
    void FitTheScreen() {
        float screenHeight = Camera.main.orthographicSize * 2;
        float screenWidth = screenHeight * Screen.width / Screen.height;

        float spriteHeight = spriteRenderer.sprite.bounds.size.y;
        float spriteWidth = spriteRenderer.sprite.bounds.size.x;

        float ratioWidth = screenWidth / spriteWidth;
        float ratioHeight = screenHeight / spriteHeight;

        float ratio = ratioWidth > ratioHeight ? ratioWidth : ratioHeight;

        Vector3 scale = transform.localScale;
        scale.x = ratio;
        scale.y = ratio;

        transform.localScale = scale;
    }

    void Start() {
        context = SynchronizationContext.Current;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        Sprite[] bgSprites = Resources.LoadAll<Sprite>("Images");
        if (bgSprites != null && bgSprites.Length > 0) {
            int indexBg = UnityEngine.Random.Range(0, bgSprites.Length);
            background = bgSprites[indexBg];
            spriteRenderer.sprite = background;
        }
        FitTheScreen();

    }

    void OnApplicationQuit() {
        // TODO: On Application quit
    }

    void OnApplicationPause(bool pauseStatus) {
        // TODO: Check "in_game" status to go to match scene
        if (pauseStatus) {
            Debug.Log("App is paused (background mode)");
        }
        else {
            Debug.Log("App is resumed");
        }
    }

    public void Initialize() { }

    public void UpdateAppState(AppState newState) {
        appState = newState;
        OnAppStateChanged?.Invoke(appState);
    }
    public AppState UpdateAppState(Func<AppState, AppState> action) {
        appState = action(appState);
        OnAppStateChanged?.Invoke(appState);
        return appState;
    }

    IEnumerator CountUpCoroutine() {
        UpdateGameState(new GameState {
            status = GameState.Status.findingMatch,
            data = new FindingMatchState {
                secondsElapsed = 0,
            }
        });

        while (gameState.status == GameState.Status.findingMatch) {
            yield return new WaitForSeconds(1f);
            gameState.data.secondsElapsed++;
            UpdateGameState(gameState);
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

    public void StartCountUp() {
        StartCoroutine(CountUpCoroutine());
    }
    public void StopCountUp() {
        gameState = new GameState {
            status = GameState.Status.active,
            data = new FindingMatchState(),
        };
    }
}

