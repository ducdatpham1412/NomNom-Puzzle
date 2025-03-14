using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

using Answer = RoomMath.Question.Answer;
using Question = RoomMath.Question;

public class MathManager : NetworkBehaviour {
    [Header("GameObjects")]
    public GameObject MatchScore;
    public LocalizeStringEvent c_TextNotice;
    public Image c_Hover;

    [Header("MathBoard")]
    public GameObject MathBoard;
    public Text Question;
    public Text LeftAnswer;
    public Text RightAnswer;

    [Header("MathScore")]
    public GamePoints GamePointAbove;
    public GamePoints GamePointsBelow;

    MatchState matchState;
    RoomMath roomMath;
    SynchronizationContext context;

    List<int> s_ConnectedClients = new List<int>();
    TaskCompletionSource<string> s_tsc;
    Question s_currentQuestion;
    int s_maxPoint = 7;

    bool c_selected = false;
    string c_myID = "";

    void Start() {
        context = SynchronizationContext.Current;
        SoundManager.Instance.PlayMusic(SoundManager.MusicSource.mathBeat);
    }

    public override void OnNetworkSpawn() {
        matchState = GameManager.Instance.gameState.matchState;
        roomMath = matchState.rooms.Find(r => r["status"].ToString() == "active").ToObject<RoomMath>();

        if (IsClient) {
            ReadyServerRpc();
            c_myID = GameManager.Instance.appState.profile.id;
        }
        else if (IsServer) {
            SocketManager.OnHandleData += S_LoadMatch;
        }


        InitUI();
    }

    public override void OnNetworkDespawn() {
        SoundManager.Instance.PauseUnPauseMusicBackground();
        if (IsServer) {
            SocketManager.OnHandleData -= S_LoadMatch;
        }
    }

    void C_ScaleUI(GamePoints Point) {
        ShakeManager shake = Point.gameObject.GetComponent<ShakeManager>();
        shake.StartScale();
    }

    void S_LoadMatch(JObject evt) {
        string eventType = evt["type"].ToString();
        if (eventType == "get_more_ques") {
            Question[] newQuestions = evt["data"].ToObject<Question[]>();
            roomMath.questions.AddRange(newQuestions);
            if (s_tsc != null) {
                s_tsc.SetResult("Done");
                s_tsc = null;
            }
        }
    }

    IEnumerator FadeOutHover() {
        Color color = c_Hover.color;
        float originalA = color.a;
        float elapsedTime = 0f;
        float duration = 0.7f;
        while (elapsedTime < duration) {
            float alpha = Mathf.Lerp(originalA, 0f, elapsedTime / duration);
            c_Hover.color = new Color(color.r, color.g, color.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        c_Hover.color = new Color(color.r, color.g, color.b, 0f);
        c_Hover.gameObject.SetActive(false);
    }

    async Task ShowNotice(string key, int duration = 3000, int fontSize = 30) {
        c_TextNotice.StringReference.SetReference(LocalizationManager.Table.Game.ToString(), key);
        Text text = c_TextNotice.gameObject.GetComponent<Text>();
        text.fontSize = fontSize;
        c_TextNotice.gameObject.SetActive(true);
        await Task.Delay(duration);
        c_TextNotice.gameObject.SetActive(false);
    }

    void InitUI() {
        foreach (var player in matchState.players) {
            if (player.id == c_myID) {
                GamePointsBelow.TextValue.text = player.name;
            }
            else {
                GamePointAbove.TextValue.text = player.name;
            }
        }
        MatchScore.GetComponent<CanvasGroup>().alpha = 1;
    }

    public void C_PressSelection(int index) {
        if (!c_selected) {
            c_selected = true;
            SelectIndexServerRpc(index);
        }
    }

    async void S_WaitForNextRound() {
        int currentIndex = roomMath.questions.FindIndex(q => q.id == s_currentQuestion.id);
        bool hasMoreQues = currentIndex < roomMath.questions.Count - 1;
        if (!hasMoreQues) {
            s_tsc = new TaskCompletionSource<string>();
            SocketManager.Send(new Event.Send.GetMoreQuestion {
                last_id = s_currentQuestion.id,
            });
            await s_tsc.Task;
        }
        s_currentQuestion = roomMath.questions[currentIndex + 1];
        await Task.Delay(1500);
        ShowMathBoardClientRpc(s_currentQuestion);
    }

    async void S_InitQuesAndShowMathBoard() {
        Question question = roomMath.questions.Find(q => q.status == RoomMath.Question.Status.active.ToString());
        Question.text = question.ques;
        LeftAnswer.text = question.selections[0].ToString();
        RightAnswer.text = question.selections[1].ToString();
        MathBoard.GetComponent<CanvasGroup>().alpha = 1;
        s_currentQuestion = question;
        await Task.Delay(4500);
        ShowMathBoardClientRpc(s_currentQuestion);
    }

    void S_CheckContinueOrEndGame(RoomMath.Player winner) {
        RoomMath.Player loser = roomMath.players.Find(p => p.id != winner.id);

        if (winner.point == s_maxPoint) {
            if (winner.point - loser.point >= 2) {
                NotifyWinnerClientRpc(winner.id, winner.point);
                S_EndGame();
                return;
            }
            // Winner get max but only more than lose "1 point"
            winner.point -= 1;
            loser.point -= 1;
        }

        NotifyResultClientRpc(
            winnerID: winner.id,
            winnerPoint: winner.point,
            loserPoint: loser.point
        );
        S_WaitForNextRound();
    }

    async void S_EndGame() {
        roomMath.status = BaseRoom.Status.ended.ToString();
        for (int i = 0; i < matchState.rooms.Count; i++) {
            if (matchState.rooms[i]["id"].ToString() == roomMath.id) {
                matchState.rooms[i] = JObject.FromObject(roomMath);
                break;
            }
        }
        foreach (MatchState.Player player in matchState.players) {
            player.status = MatchState.Player.Status.active.ToString();
        }
        await Task.Delay(2500);
        Navigator.Instance.NetworkLoad(Navigator.Scene.MatchScene);
    }

    [ClientRpc]
    void IntroduceRoomClientRpc() {
        context.Post(async _ => {
            await ShowNotice("whoSolveFaster", 2000);
            await ShowNotice("areYouReady", 1500);
            await ShowNotice("fight", 1000, 50);
            StartCoroutine(FadeOutHover());
        }, null);
    }

    [ClientRpc]
    void ShowMathBoardClientRpc(Question question) {
        c_selected = false;
        context.Post(_ => {
            Question.text = question.ques;
            LeftAnswer.text = question.selections[0].ToString();
            RightAnswer.text = question.selections[1].ToString();
            MathBoard.GetComponent<CanvasGroup>().alpha = 1;
        }, null);
    }

    [ClientRpc]
    void NotifyResultClientRpc(FixedString32Bytes winnerID, int winnerPoint, int loserPoint) {
        if (winnerID.ToString() == c_myID) {
            SoundManager.Instance.PlaySF(SoundManager.SF.Correct);
            GamePointsBelow.UpdatePoint(winnerPoint);
            GamePointAbove.UpdatePoint(loserPoint);
            C_ScaleUI(GamePointsBelow);
        }
        else {
            SoundManager.Instance.PlaySF(SoundManager.SF.Nope);
            GamePointAbove.UpdatePoint(winnerPoint);
            GamePointsBelow.UpdatePoint(loserPoint);
            C_ScaleUI(GamePointAbove);
        }
    }

    [ClientRpc]
    void NotifyWinnerClientRpc(FixedString32Bytes winnerID, int point) {
        if (winnerID.ToString() == c_myID) {
            SoundManager.Instance.PlaySF(SoundManager.SF.Correct);
            GamePointsBelow.UpdatePoint(point);
            C_ScaleUI(GamePointsBelow);
        }
        else {
            SoundManager.Instance.PlaySF(SoundManager.SF.Nope);
            GamePointAbove.UpdatePoint(point);
            C_ScaleUI(GamePointAbove);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    void ReadyServerRpc(ServerRpcParams rpcParams = default) {
        ulong clientID = rpcParams.Receive.SenderClientId;
        var allClients = matchState.players.Select(p => p.clientID).ToArray();
        if (!allClients.Contains((int)clientID)) {
            return;
        }

        s_ConnectedClients.Add((int)clientID);
        bool allReady = true;
        foreach (MatchState.Player p in matchState.players) {
            if (!s_ConnectedClients.Contains(p.clientID)) {
                allReady = false;
            }
        }

        if (allReady) {
            StartCoroutine(FadeOutHover());
            IntroduceRoomClientRpc();
            S_InitQuesAndShowMathBoard();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SelectIndexServerRpc(int index, ServerRpcParams rpcParams = default) {
        ulong clientID = rpcParams.Receive.SenderClientId;
        MatchState.Player fPlayer = matchState.players.Find(p => p.clientID == (int)clientID);
        if (fPlayer != null) {
            RoomMath.Player fMathPlayer = roomMath.players.Find(p => p.id == fPlayer.id);
            Debug.Log("Check: " + fMathPlayer.id);
            Answer answer = new Answer {
                select = s_currentQuestion.selections[index],
                creator = fMathPlayer.id,
                created = Helper.TsNow(),
            };
            bool shouldCheckWinner = s_currentQuestion.answers.Count == 0;
            s_currentQuestion.answers.Add(answer);
            if (shouldCheckWinner) {
                if (answer.select != s_currentQuestion.result) {
                    fMathPlayer = roomMath.players.Find(p => p.id != fPlayer.id);
                }
                s_currentQuestion.winner = fMathPlayer.id;
                s_currentQuestion.status = RoomMath.Question.Status.ended.ToString();
                fMathPlayer.point += 1;

                S_CheckContinueOrEndGame(fMathPlayer);
            }
        }
    }
}
