using System;
using System.Collections;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class GameManager : Singleton<GameManager> {
    protected GameManager() { }

    public Sprite background;
    SpriteRenderer spriteRenderer;
    SynchronizationContext context;
    Coroutine matchFoundSound;


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

        SocketManager.OnHandleData += HandleSocketEvent;
    }

    // This function for common socket event, which need to handle without depending on current scene on app
    void HandleSocketEvent(JObject evt) {
        string eventType = evt["type"]?.ToString();

        if (eventType == Event.Name.match_found.ToString()) {
            MatchState match = JsonConvert.DeserializeObject<MatchState>(evt["data"].ToString());
            context.Post(_ => {
                StopCountUp();
                UpdateAppState(state => {
                    state.client.match_ip = match.configs.ip;
                    state.client.match_port = match.configs.port;
                    return state;
                });
                matchFoundSound = StartCoroutine(MatchFoundSound());
            }, null);
            return;
        }

        if (eventType == Event.Name.server_ready.ToString()) {
            context.Post(_ => {
                UpdateGameState(state => {
                    state.status = GameState.Status.inGame;
                    return state;
                });
                StopCoroutine(matchFoundSound);
                Navigator.Instance.NavigateTo(Navigator.Scene.MatchScene);
            }, null);
            return;
        }
    }

    IEnumerator MatchFoundSound() {
        int count = 0;
        while (count <= 3) {
            SoundManager.Instance.PlaySF(SoundManager.SF.NewTing);
            count++;
            yield return new WaitForSeconds(1.5f);
        }
    }

    void OnApplicationQuit() {
        SocketManager.Disconnected();
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

