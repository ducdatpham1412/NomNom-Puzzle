using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Collections;
using System.Collections;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Linq;


public class ThrowEggLogic : NetworkBehaviour {
    [Header("Prefabs")]
    public GameObject EggPrefab;
    public GameObject PlayerPrefab;

    [Header("GameObjects")]
    public GameObject MatchScore;
    public LocalizeStringEvent c_TextNotice;
    public Image c_Hover;

    [Header("NetworkVariable")]
    public NetworkVariable<FixedString32Bytes> m_TargetID = new NetworkVariable<FixedString32Bytes>("");

    RoomThrowEgg roomThrowEgg;
    MatchState matchState;
    SynchronizationContext context;

    List<int> s_ConnectedClients = new List<int>();
    Dictionary<string, PlayerThrowEggManager> s_Players = new Dictionary<string, PlayerThrowEggManager>();
    GameObject s_Egg;

    void Start() {
        context = SynchronizationContext.Current;
        SoundManager.Instance.PlayMusic(SoundManager.MusicSource.lifeWandering);
    }

    public override void OnNetworkDespawn() {
        SoundManager.Instance.PauseUnPauseMusicBackground();
    }

    public override void OnNetworkSpawn() {
        matchState = GameManager.Instance.gameState.matchState;
        roomThrowEgg = matchState.rooms.Find(r => r["status"].ToString() == "active").ToObject<RoomThrowEgg>();
        if (IsClient) {
            ReadyServerRpc();
            C_ModifyCameraView();
        }
    }

    void FlipGameObject(GameObject gameObject) {
        gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 180f));
        Vector3 pos = gameObject.transform.position;
        pos.y = pos.y - 1;
        gameObject.transform.position = pos;
    }

    void C_ModifyCameraView() {
        RoomThrowEgg.Player player = roomThrowEgg.players.Find(p => p.id == GameManager.Instance.appState.profile.id);
        if (player != null && player.init_pos.y > 0) {
            GameObject MainCamera = GameObject.Find("MainCamera");
            GameObject BackgroundHover = GameObject.Find("BackgroundHover");
            FlipGameObject(MainCamera);
            FlipGameObject(BackgroundHover);
        }
    }

    void S_InitEgg(PlayerThrowEggManager playerManager) {
        int ownerClientID = matchState.players.Find(p => p.id == playerManager.m_Player.Value.id).clientID;
        bool isUnder = playerManager.m_Player.Value.init_pos.y < 0;
        int eggID = 1;
        if (s_Egg != null) {
            EggManager lastManager = s_Egg.GetComponent<EggManager>();
            eggID = lastManager.m_Egg.Value.id + 1;
        }
        s_Egg = Instantiate(EggPrefab, playerManager.m_Player.Value.init_pos, isUnder ? Quaternion.identity : Quaternion.Euler(180, 0, 0));
        EggManager manager = s_Egg.GetComponent<EggManager>();
        manager.Initialize(
            id: eggID,
            creatorID: playerManager.m_Player.Value.id,
            move_speed: playerManager.m_Player.Value.move_speed,
            shot_speed: playerManager.m_Player.Value.shot_speed * (isUnder ? 1 : -1), _logic: this,
            ownerClientID: ownerClientID < 0 ? NetworkManager.ServerClientId : (ulong)ownerClientID
        );
    }

    FixedString32Bytes S_InitObjects() {
        PlayerThrowEggManager InitPlayer(RoomThrowEgg.Player player) {
            GameObject newGameObject = Instantiate(PlayerPrefab, player.init_pos, Quaternion.identity);
            PlayerThrowEggManager manager = newGameObject.GetComponent<PlayerThrowEggManager>();
            manager.Initialize(player, this);
            s_Players.Add(player.id, manager);
            return manager;
        }

        m_TargetID.Value = roomThrowEgg.players[1].id;
        PlayerThrowEggManager manager01 = InitPlayer(roomThrowEgg.players[0]);
        InitPlayer(roomThrowEgg.players[1]);
        S_InitEgg(manager01);

        return m_TargetID.Value;
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
            FixedString32Bytes initTarget = S_InitObjects();
            StartCoroutine(FadeOutHover());
            IntroduceRoomClientRpc(initTarget);
        }
    }

    [ClientRpc]
    void IntroduceRoomClientRpc(FixedString32Bytes initTarget) {
        // We have to pass initTarget instead of using m_TargetID because of the delay of the internet
        context.Post(async _ => {
            bool isMyTurnFirst = initTarget.ToString() != GameManager.Instance.appState.profile.id;
            await ShowNotice(isMyTurnFirst ? "youAreTheFirst" : "opponentBeFirst", 2000);
            await ShowNotice("areYouReady", 1500);
            SoundManager.Instance.PlaySF(SoundManager.SF.Fight);
            await ShowNotice("fight", 1000, 50);
            StartCoroutine(FadeOutHover());
        }, null);
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
        MatchScore.GetComponent<CanvasGroup>().alpha = 1;
    }

    async Task ShowNotice(string key, int duration = 3000, int fontSize = 30) {
        c_TextNotice.StringReference.SetReference(LocalizationManager.Table.Game.ToString(), key);
        Text text = c_TextNotice.gameObject.GetComponent<Text>();
        text.fontSize = fontSize;
        c_TextNotice.gameObject.SetActive(true);
        await Task.Delay(duration);
        c_TextNotice.gameObject.SetActive(false);
    }

    bool S_PlusPointAndCheckEnded(PlayerThrowEggManager Shot, PlayerThrowEggManager Target) {
        int newPoint = Shot.m_Player.Value.point + 1;
        bool equalTurn = roomThrowEgg.eggs.Count % 2 == 0;
        if (newPoint == 5) {
            if (equalTurn) {
                if (Target.m_Player.Value.point < 5) {
                    Shot.UpdatePoint(newPoint);
                    return true; // ended
                }
                //Both 2 players have 5 points, reset to 3 point for all
                Shot.UpdatePoint(3);
                Target.UpdatePoint(3);
                return false;
            }
            if ((newPoint - Target.m_Player.Value.point) >= 2) {
                Shot.UpdatePoint(newPoint);
                return true;
            }
        }
        Shot.UpdatePoint(newPoint);
        return false;
    }

    async void S_SwitchTurn(bool isHit) {
        PlayerThrowEggManager Target = s_Players[m_TargetID.Value.ToString()];
        string newTarget = m_TargetID.Value.ToString();
        string winnerID = "";

        foreach (KeyValuePair<string, PlayerThrowEggManager> kpv in s_Players) {
            if (kpv.Key != m_TargetID.Value.ToString()) {
                newTarget = kpv.Key;
                PlayerThrowEggManager Shot = kpv.Value;
                if (isHit) {
                    bool ended = S_PlusPointAndCheckEnded(Shot, Target);
                    if (ended) {
                        winnerID = Shot.m_Player.Value.id;
                    }
                }
                else if (Target.m_Player.Value.point == 5) {
                    winnerID = Target.m_Player.Value.id;
                }
            }
        }
        await Task.Delay(1300);
        NetworkObject network = s_Egg.GetComponent<NetworkObject>();
        network.Despawn();
        m_TargetID.Value = newTarget;
        if (winnerID != "") {
            S_EndGame(winnerID);
        }
        else {
            S_InitEgg(Target);
        }
    }

    public void S_SendResult(bool isHit) {
        EggManager manager = s_Egg.GetComponent<EggManager>();
        manager.m_Egg.Value.status = isHit ? "hit" : "missed";
        roomThrowEgg.eggs.Add(manager.m_Egg.Value);
        S_SwitchTurn(isHit);
    }


    [ClientRpc]
    public void C_ShowWinnerClientRpc(string winnerID) {
        Debug.Log($"Winner winner: {winnerID}");
        // TODO: Winner UI
    }

    async void S_EndGame(string winnerID) {
        foreach (RoomThrowEgg.Player player in roomThrowEgg.players) {
            player.CopyProperties(s_Players[player.id].m_Player.Value);
            s_Players[player.id].gameObject.GetComponent<NetworkObject>().Despawn();
        }
        roomThrowEgg.status = BaseRoom.Status.ended.ToString();

        for (int i = 0; i < matchState.rooms.Count; i++) {
            if (matchState.rooms[i]["id"].ToString() == roomThrowEgg.id) {
                matchState.rooms[i] = JObject.FromObject(roomThrowEgg);
                break;
            }
        }

        // Reset status of all player to "active", prepare for next room or ending match
        foreach (MatchState.Player player in matchState.players) {
            player.status = MatchState.Player.Status.active.ToString();
        }

        C_ShowWinnerClientRpc(winnerID);
        await Task.Delay(2500);
        Navigator.Instance.NetworkLoad(Navigator.Scene.MatchScene);
    }
}
