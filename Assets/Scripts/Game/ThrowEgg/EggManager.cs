using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class EggManager : NetworkBehaviour {
    public Sprite ExplodedSprite;
    public ShakeManager shakeManager;
    Rigidbody2D Rigid;
    SpriteRenderer Renderer;
    ClientNetworkTransform networkTransform;
    MoveManager moveManager;
    ThrowEggLogic s_throwEggLogic;
    bool s_shouldCheckMissed = false;
    bool c_hit = false;
    public NetworkVariable<RoomThrowEgg.Egg> m_Egg = new NetworkVariable<RoomThrowEgg.Egg>();
    NetworkVariable<bool> m_Moving = new NetworkVariable<bool>(false, writePerm: NetworkVariableWritePermission.Owner);

    void Awake() {
        Rigid = GetComponent<Rigidbody2D>();
        Renderer = shakeManager.gameObject.GetComponent<SpriteRenderer>();
        networkTransform = GetComponent<ClientNetworkTransform>();
        moveManager = GetComponent<MoveManager>();
        moveManager.enabled = false;
    }

    void Start() {
        moveManager.TouchRelease = HandleShot;
    }

    public override void OnNetworkSpawn() {
        if (IsOwner) {
            moveManager.enabled = true;
            moveManager.moveSpeed = m_Egg.Value.move_speed;
            m_Egg.OnValueChanged += (_, newValue) => {
                moveManager.moveSpeed = newValue.move_speed;
            };
            moveManager.TouchBegin = () => {
                m_Moving.Value = true;
            };
        }

        m_Moving.OnValueChanged += (_, newValue) => {
            if (newValue) {
                shakeManager.StartShake();
                SoundManager.Instance.PlaySF(SoundManager.SF.Stretch);
            }
            else {
                shakeManager.StopShake();
            }
        };
    }

    void Update() {
        HandleCheckMissed();
    }

    public void Initialize(int id, string creatorID, float move_speed, float shot_speed, ThrowEggLogic _logic, ulong ownerClientID) {
        s_throwEggLogic = _logic;
        m_Egg.Value = new RoomThrowEgg.Egg {
            id = id,
            move_speed = move_speed,
            shot_speed = shot_speed,
            created = Helper.TsNow(),
            creator = creatorID,
            status = "active",
        };
        NetworkObject network = GetComponent<NetworkObject>();
        network.SpawnWithOwnership(ownerClientID);
        networkTransform.DeltaDistanceLimit = move_speed / 8;
    }

    void OnTriggerEnter2D(Collider2D collider) {
        PlayerThrowEggManager manager = collider.gameObject.GetComponent<PlayerThrowEggManager>();

        if (manager == null) {
            return;
        }

        if (!IsServer) {
            if (m_Egg.Value.status == null || m_Egg.Value.creator == manager.m_Player.Value.id) {
                return;
            }
            if (!c_hit) {
                Renderer.enabled = false;
            }
            return;
        }

        if (manager.m_Player.Value.id == s_throwEggLogic.m_TargetID.Value.ToString()) {
            s_shouldCheckMissed = false;
            Rigid.linearVelocity = Vector2.zero;
            ExplodeClientRpc(
                explodedPos: transform.position,
                hitSF: SoundManager.SF.Splat,
                reactSF: Helper.GetRandomInArr(new SoundManager.SF[] { SoundManager.SF.NiceShot, SoundManager.SF.None })
            );
            s_throwEggLogic.S_SendResult(true);
        }
    }

    [ClientRpc]
    void ExplodeClientRpc(Vector3 explodedPos, SoundManager.SF hitSF, SoundManager.SF reactSF) {
        c_hit = true;
        transform.position = explodedPos;
        Renderer.enabled = true;
        Renderer.sprite = ExplodedSprite;
        Rigid.linearVelocity = Vector2.zero;
        transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
        C_PlaySF(hitSF, reactSF);
    }

    async void C_PlaySF(SoundManager.SF hitSF, SoundManager.SF reactSF) {
        SoundManager.Instance.PlaySF(hitSF);
        await Task.Delay(500);
        SoundManager.Instance.PlaySF(reactSF);
    }

    void M_Shot() {
        networkTransform.enabled = false;
        Rigid.linearVelocity = new Vector2(0f, m_Egg.Value.shot_speed);
        SoundManager.Instance.PlaySF(SoundManager.SF.Whoosh);
    }

    void HandleShot() {
        ShotServerRpc();
        M_Shot();
    }

    [ServerRpc]
    void ShotServerRpc(ServerRpcParams rpcParams = default) {
        NetworkObject network = GetComponent<NetworkObject>();
        network.ChangeOwnership(NetworkManager.ServerClientId);
        m_Moving.Value = false;
        ShotBroadcastClientRpc(new ClientRpcParams {
            Send = new ClientRpcSendParams {
                TargetClientIds = NetworkManager.ConnectedClientsIds.Where(C => C != rpcParams.Receive.SenderClientId).ToList(),
            }
        });
        s_shouldCheckMissed = true;

        // TODO: Create class GameSnapshot and Rewind Lag Compensation
        M_Shot();
    }


    [ClientRpc]
    void ShotBroadcastClientRpc(ClientRpcParams rpcParams) {
        M_Shot();
    }

    private void HandleCheckMissed() {
        if (!s_shouldCheckMissed || !IsServer) {
            return;
        }
        float posY = Rigid.position.y;
        if (posY > GameHelper.world.maxY || posY < -GameHelper.world.maxY) {
            s_shouldCheckMissed = false;
            PlayNopeClientRpc();
            s_throwEggLogic.S_SendResult(false);
        }
    }

    [ClientRpc]
    void PlayNopeClientRpc() {
        SoundManager.Instance.PlaySF(SoundManager.SF.Nope);
    }
}
