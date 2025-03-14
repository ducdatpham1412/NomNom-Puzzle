using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerThrowEggManager : NetworkBehaviour {
    GamePoints gamePoints;
    SpriteRenderer Renderer;
    ClientNetworkTransform networkTransform;
    MoveManager moveManager;
    BoxCollider2D boxCollider;
    bool c_setAvatar = false;
    public NetworkVariable<RoomThrowEgg.Player> m_Player = new NetworkVariable<RoomThrowEgg.Player>();
    NetworkVariable<bool> m_enable = new NetworkVariable<bool>(false);

    void Awake() {
        Renderer = GetComponent<SpriteRenderer>();
        networkTransform = GetComponent<ClientNetworkTransform>();
        moveManager = GetComponent<MoveManager>();
        boxCollider = GetComponent<BoxCollider2D>();

        Renderer.enabled = m_enable.Value;
        boxCollider.enabled = m_enable.Value;
    }

    public override void OnNetworkSpawn() {
        m_enable.OnValueChanged += OnEnableChanged;
        m_Player.OnValueChanged += OnPlayerChanged;

        OnEnableChanged(m_enable.Value, m_enable.Value);

        if (IsOwner) {
            moveManager.enabled = true;
            moveManager.moveSpeed = m_Player.Value.move_speed;
        }
    }

    void OnEnableChanged(bool _, bool newValue) {
        Renderer.enabled = newValue;
        boxCollider.enabled = newValue;
    }

    void S_UpdateEnable(FixedString32Bytes targetID) {
        m_enable.Value = targetID.ToString() == m_Player.Value.id;
    }

    public void Initialize(RoomThrowEgg.Player player, ThrowEggLogic _eggLogic) {
        MatchState.Player fPlayer = GameManager.Instance.gameState.matchState.players.Find(p => p.id == player.id);
        if (fPlayer != null) {
            m_Player.Value = player;
            S_UpdateEnable(_eggLogic.m_TargetID.Value);
            _eggLogic.m_TargetID.OnValueChanged += OnTargetChanged;
            NetworkObject network = GetComponent<NetworkObject>();
            network.SpawnWithOwnership(fPlayer.clientID < 0 ? NetworkManager.ServerClientId : (ulong)fPlayer.clientID);
            moveManager.moveSpeed = player.move_speed;
            networkTransform.DeltaDistanceLimit = player.move_speed / 8;
        }
    }

    public void UpdatePoint(int newPoint) {
        m_Player.Value = new RoomThrowEgg.Player {
            id = m_Player.Value.id,
            move_speed = m_Player.Value.move_speed,
            shot_speed = m_Player.Value.shot_speed,
            point = newPoint,
            init_pos = m_Player.Value.init_pos,
        };
    }

    void OnTargetChanged(FixedString32Bytes _, FixedString32Bytes newValue) {
        S_UpdateEnable(newValue);
    }

    async void SetAvatar(string url) {
        c_setAvatar = true;
        Sprite sprite = await Helper.ImgUrlToSprite(url);
        if (sprite != null) {
            GetComponent<SpriteRenderer>().sprite = sprite;
        }
        // Helper.FitSpriteToGameObject(gameObject);
    }

    void GetGamePoint(RoomThrowEgg.Player player) {
        GameObject Canvas = GameObject.Find("Canvas");
        Transform MatchScore = Helper.FindChildRecursive(Canvas.transform, "MatchScore");
        if (MatchScore != null) {
            bool isUnder = player.id == GameManager.Instance.appState.profile.id;
            Transform PointObject = Helper.FindChildRecursive(MatchScore, isUnder ? "PointDown" : "PointUp");
            gamePoints = PointObject.GetComponent<GamePoints>();
            MatchState.Player fPlayer = GameManager.Instance.gameState.matchState.players.Find(p => p.id == player.id);
            gamePoints.TextValue.text = fPlayer.name;
        }
    }

    void UpdateScoreUI(RoomThrowEgg.Player player) {
        if (gamePoints == null) {
            GetGamePoint(player);
        }

        if (!c_setAvatar) {
            MatchState.Player fPlayer = GameManager.Instance.gameState.matchState.players.Find(p => p.id == player.id);
            SetAvatar(fPlayer.avatar);
        }

        gamePoints.UpdatePoint(player.point);
    }

    void OnPlayerChanged(RoomThrowEgg.Player oldValue, RoomThrowEgg.Player newValue) {
        // Because Network Navigate will update in server immediately but in Client, it will delay, and still have scene MatchScene before, so we must wait until GameThrowEgg scene is loaded
        if (IsClient) {
            UpdateScoreUI(newValue);
            if (newValue.point > oldValue.point) {
                Scale();
            }
        }
        if (IsOwner) {
            moveManager.moveSpeed = m_Player.Value.move_speed;
        }
    }

    void Scale() {
        ShakeManager shake = gamePoints.gameObject.GetComponent<ShakeManager>();
        shake.StartScale();
    }
}
