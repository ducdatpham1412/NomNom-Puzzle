using System.Collections;
using System.Threading;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Localization.Components;

public class FindMatchButton : MonoBehaviour {
    public LocalizeStringEvent ButtonTitle;
    SynchronizationContext context;

    void Start() {
        context = SynchronizationContext.Current;
        GameState gameState = GameManager.Instance.gameState;
        if (gameState.status == GameState.Status.findingMatch) {
            ButtonTitle.StringReference.SetReference(LocalizationManager.Table.Home.ToString(), "finding");
            UpdateTextCountUp(gameState.data.secondsElapsed);
            GameManager.Instance.OnGameStateChanged += ListenCountUp;
        }
        SocketManager.OnHandleData += MatchFound;
    }

    void OnDestroy() {
        GameManager.Instance.OnGameStateChanged -= ListenCountUp;
        SocketManager.OnHandleData -= MatchFound;
    }

    void ListenCountUp(GameState newState) {
        UpdateTextCountUp(newState.data.secondsElapsed);
    }

    void UpdateTextCountUp(int secondsElapsed) {
        int minutes = secondsElapsed / 60;
        int seconds = secondsElapsed % 60;
        string formattedTime = $"{minutes:00}:{seconds:00}";
        ButtonTitle.StringReference.Arguments = new object[] { formattedTime };
        ButtonTitle.RefreshString();
    }

    void MatchFound(JObject evt) {
        if (evt["type"]?.ToString() == Event.Name.match_found.ToString()) {
            context.Post(_ => {
                ButtonTitle.StringReference.SetReference(LocalizationManager.Table.Home.ToString(), "matchFound");
                StartCoroutine(Twink());
            }, null);
        }
    }

    IEnumerator Twink() {
        int count = 0;
        bool changedText = false;
        while (true) {
            ButtonTitle.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.3f);
            ButtonTitle.gameObject.SetActive(true);
            yield return new WaitForSeconds(1.2f);
            if (count <= 3) {
                count++;
            }
            else if (!changedText) {
                ButtonTitle.StringReference.SetReference(LocalizationManager.Table.Home.ToString(), "readyForMatch");
                changedText = true;
            }
        }
    }

    public void FindMatch() {
        if (GameManager.Instance.gameState.status == GameState.Status.findingMatch) {
            return;
        }
        ButtonTitle.StringReference.SetReference(LocalizationManager.Table.Home.ToString(), "finding");
        GameManager.Instance.OnGameStateChanged += ListenCountUp;
        GameManager.Instance.StartCountUp();
        SocketManager.Send(new Event.Send.FindMatch());
    }

    public void StopFinding() {
        ButtonTitle.StringReference.SetReference(LocalizationManager.Table.Home.ToString(), "findMatch");
        GameManager.Instance.OnGameStateChanged -= ListenCountUp;
        GameManager.Instance.StopCountUp();
        SocketManager.Send(new Event.Send.StopFindMatch());
    }
}
