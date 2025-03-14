using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class MatchManager : NetworkBehaviour {
    [Header("GameObjects")]
    public RectTransform Content;
    public GameObject CardPrefab;
    public GameObject Line;
    public GameObject FinalLeaderBoard;
    public GameObject BackToLobby;

    string READY_STATUS = MatchState.Player.Status.ready.ToString();
    bool hasLoadedMatch = false;
    bool ended = false;

    AppState appState;
    GameState gameState;
    SynchronizationContext context;
    List<RoomCardManager> roomManagers = new List<RoomCardManager>();

    void Start() {
        appState = GameManager.Instance.appState;
        gameState = GameManager.Instance.gameState;
        context = SynchronizationContext.Current;
        InitGame();
    }

    public override void OnNetworkSpawn() {
        if (IsClient) {
            // We have to check like on Start for comeback from room to match, OnNetworkSpawn will run before Start
            if (appState == null) {
                appState = GameManager.Instance.appState;
                gameState = GameManager.Instance.gameState;
                context = SynchronizationContext.Current;
            }
            LoadMatchServerRpc(appState.profile.id);
            return;
        }

        if (IsServer) {
            Application.logMessageReceived += S_HandleLog;
        }
    }

    public override void OnNetworkDespawn() {
        if (IsServer) {
            SocketManager.OnHandleData -= S_LoadMatch;
            Application.logMessageReceived -= S_HandleLog;
        }
    }

    void S_HandleLog(string logString, string stackTrace, LogType type) {
        if (type == LogType.Exception) {
            Debug.Log($"ERROR CRASH APP: {logString}");
            Application.Quit();
        }
    }

    void S_LoadMatch(JObject evt) {
        string eventType = evt["type"].ToString();

        if (eventType == "load_match") {
            gameState.status = GameState.Status.inGame;
            gameState.matchState = evt["data"].ToObject<MatchState>();
            SocketManager.Send(new Event.Send.ServerReady());
            context.Post(_ => {
                DisplayCards();
            }, null);
        }
    }

    void DisplayCards() {
        if (ended) return;

        if (!hasLoadedMatch) {
            for (int i = 0; i < gameState.matchState.rooms.Count; i++) {
                JObject room = gameState.matchState.rooms[i];
                GameObject NewCard = Instantiate(CardPrefab);
                NewCard.transform.SetParent(Content, false);

                RoomCardManager manager = NewCard.GetComponent<RoomCardManager>();
                manager.Initialize(room["id"].ToString());
                roomManagers.Add(manager);
            }
            hasLoadedMatch = true;
        }

        if (IsClient) {
            CheckContinueOrEndGame();
        }
    }

    private void S_InitServer(string match_id, ushort match_port) {
        SocketManager.OnHandleData += S_LoadMatch;
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData("0.0.0.0", match_port);
        NetworkManager.Singleton.StartServer();
        SocketManager.Send(new Event.Send.LoadMatch { match_id = match_id });
        Debug.Log($"INITIALIZED SERVER {match_id} AT PORT: {match_port}");
    }

    [ServerRpc(RequireOwnership = false)]
    private void LoadMatchServerRpc(string playerID, ServerRpcParams rpcParams = default) {
        foreach (MatchState.Player player in gameState.matchState.players) {
            if (player.id == playerID) {
                player.status = READY_STATUS;
                player.clientID = (int)rpcParams.Receive.SenderClientId;
            }
        }
        PassMatchStateClientRpc(gameState.matchState);
        CheckContinueOrEndGame();
    }

    [ClientRpc]
    void PassMatchStateClientRpc(MatchState state) {
        gameState.matchState = state;
        context.Post(_ => {
            DisplayCards();
        }, null);
    }

    async void CheckContinueOrEndGame() {
        bool allReady = gameState.matchState.players.Find(p => p.status != READY_STATUS) == null;
        if (allReady) {
            JObject room = gameState.matchState.rooms.Find(r => r["status"].ToString() == BaseRoom.Status.active.ToString());

            // Case 1: Still having active room in mathState => continue
            if (room != null) {
                string roomID = room["id"].ToString();
                while (!hasLoadedMatch) {
                    await Task.Delay(50);
                }
                RoomCardManager roomCard = roomManagers.Find(r => r.roomID == roomID);
                roomCard.CountDownToBegin();
                return;
            }

            // Case 2: All rooms end => Endgame
            EndGame();
        }
    }

    void C_InitClient() {
        string IP = appState.client.match_ip;
        int? PORT = appState.client.match_port;
        if (IP != "" && PORT != null) {
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport != null) {
                transport.SetConnectionData(IP, (ushort)PORT);
                NetworkManager.Singleton.StartClient();
            }
        }
    }

    void InitGame() {
        if (!NetworkManager.Singleton.IsListening) {
            string match_id = Configs.Env.match_id;
            string match_port = Configs.Env.match_port;

            if (match_id != null && match_port != null) {
                try {
                    ushort port = ushort.Parse(match_port);
                    // Must set profile id of server to be match_id for UDP socket validate
                    GameManager.Instance.appState.profile.id = match_id;
                    S_InitServer(match_id, port);
                    return;
                }
                catch (Exception e) {
                    Debug.Log($"ERROR CRASH APP: {e.Message}");
                    Application.Quit();
                }

            }

            C_InitClient();
        }
        else if (IsServer) {
            DisplayCards();
        }
    }


    void EndGame() {
        if (IsClient) {
            ended = true;
            NetworkManager.Singleton.Shutdown();
            GameObject LineObject = Instantiate(Line);
            GameObject Final = Instantiate(FinalLeaderBoard);
            LineObject.transform.SetParent(Content, false);
            Final.transform.SetParent(Content, false);
            Final.GetComponent<FinalLeaderboard>().SetData(gameState.matchState);
            BackToLobby.transform.SetAsLastSibling();
            BackToLobby.SetActive(true);
            return;
        }

        if (IsServer) {
            ConnectManager connectManager = GetComponent<ConnectManager>();
            connectManager.S_EndGame(MatchState.Status.ended);
        }
    }

    public void GoBackLobby() {
        Navigator.Instance.NavigateTo(Navigator.Scene.MatchMaking);
    }
}
